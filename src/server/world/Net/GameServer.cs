using Arise.Server.Bridge;
using Arise.Server.Net.Sessions;

namespace Arise.Server.Net;

[RegisterSingleton<GameServer>]
[SuppressMessage("", "CA1812")]
internal sealed partial class GameServer : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Game server now listening on: {EndPoint}")]
        public static partial void StartedListening(ILogger<GameServer> logger, IPEndPoint endPoint);

        [LoggerMessage(1, LogLevel.Information, "Game client connected from {EndPoint}")]
        public static partial void ClientConnected(ILogger<GameServer> logger, IPEndPoint endPoint);

        [LoggerMessage(2, LogLevel.Debug, "Game client dropped")]
        public static partial void ClientDropped(ILogger<GameServer> logger, Exception? exception);

        [LoggerMessage(3, LogLevel.Debug, "Game client from {EndPoint} dropped")]
        public static partial void ClientDropped(ILogger<GameServer> logger, Exception? exception, IPEndPoint endPoint);

        [LoggerMessage(4, LogLevel.Information, "Game client from {EndPoint} disconnected")]
        public static partial void ClientDisconnected(
            ILogger<GameServer> logger, Exception? exception, IPEndPoint endPoint);

        // TODO: https://github.com/dotnet/runtime/issues/90589

        [LoggerMessage(5, LogLevel.Trace, "C -> S {EndPoint}: Tera:{Code} ({Length} bytes)")]
        public static partial void TeraPacketReceived(
            ILogger<GameServer> logger, IPEndPoint endPoint, TeraGamePacketCode code, int length);

        [LoggerMessage(6, LogLevel.Trace, "C -> S {EndPoint}: Arise:{Code} ({Length} bytes)")]
        public static partial void ArisePacketReceived(
            ILogger<GameServer> logger, IPEndPoint endPoint, AriseGamePacketCode code, int length);

        [LoggerMessage(7, LogLevel.Trace, "S -> C {EndPoint}: Tera:{Code} ({Length} bytes)")]
        public static partial void TeraPacketSent(
            ILogger<GameServer> logger, IPEndPoint endPoint, TeraGamePacketCode code, int length);

        [LoggerMessage(8, LogLevel.Trace, "S -> C {EndPoint}: Arise:{Code} ({Length} bytes)")]
        public static partial void ArisePacketSent(
            ILogger<GameServer> logger, IPEndPoint endPoint, AriseGamePacketCode code, int length);
    }

    private readonly Queue<GameConnectionListener> _listeners = new();

    private readonly IOptions<WorldOptions> _options;

    private readonly ILogger<GameServer> _logger;

    private readonly BridgeModuleGenerator _moduleGenerator;

    private readonly ObjectPoolProvider _objectPoolProvider;

    private readonly GameServerSessionDispatcher _sessionDispatcher;

    public GameServer(
        IOptions<WorldOptions> options,
        ILogger<GameServer> logger,
        BridgeModuleGenerator moduleGenerator,
        ObjectPoolProvider objectPoolProvider,
        GameServerSessionDispatcher sessionDispatcher)
    {
        _options = options;
        _logger = logger;
        _moduleGenerator = moduleGenerator;
        _objectPoolProvider = objectPoolProvider;
        _sessionDispatcher = sessionDispatcher;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        async ValueTask<string> GetManifestResourceAsync(string name)
        {
            await using var stream = typeof(ThisAssembly).Assembly.GetManifestResourceStream(name)!;

            var buffer = GC.AllocateUninitializedArray<byte>((int)stream.Length);

            await stream.ReadExactlyAsync(buffer, cancellationToken);

            return Encoding.UTF8.GetString(buffer);
        }

        var serverPem = await GetManifestResourceAsync("arised.pem");
        var serverKey = await GetManifestResourceAsync("arised.key");

        using var caCert = X509Certificate2.CreateFromPem(await GetManifestResourceAsync("ca.pem"));
        using var serverCert = X509Certificate2.CreateFromPem(serverPem, serverKey);

        foreach (var ep in _options.Value.Endpoints)
        {
            var listener = await GameConnectionListener.CreateAsync(
                IPEndPoint.Parse(ep),
                caCert,
                serverCert,
                _moduleGenerator.GetRandomModulePair,
                _objectPoolProvider,
                cancellationToken);

            listener.ConnectionEstablished += conn =>
            {
                conn.UserState = new GameServerSession(conn);

                Log.ClientConnected(_logger, conn.EndPoint);
            };

            listener.ConnectionDropped += (_, ep, ex) =>
            {
                if (ep != null)
                    Log.ClientDropped(_logger, ex, ep);
                else
                    Log.ClientDropped(_logger, ex);
            };

            listener.ConnectionClosed += (conn, ex) => Log.ClientDisconnected(_logger, ex, conn.EndPoint);

            void LogPacket<T>(
                GameConnectionConduit conduit,
                T code,
                ReadOnlyMemory<byte> payload,
                Action<ILogger<GameServer>, IPEndPoint, T, int> logger)
                where T : unmanaged, Enum
            {
                logger(_logger, conduit.Connection.EndPoint, code, payload.Length);
            }

            listener.RawTeraPacketReceived += (conduit, code, payload) =>
                LogPacket(conduit, code, payload, Log.TeraPacketReceived);
            listener.RawArisePacketReceived += (conduit, code, payload) =>
                LogPacket(conduit, code, payload, Log.ArisePacketReceived);
            listener.RawTeraPacketSent += (conduit, code, payload) =>
                LogPacket(conduit, code, payload, Log.TeraPacketSent);
            listener.RawArisePacketSent += (conduit, code, payload) =>
                LogPacket(conduit, code, payload, Log.ArisePacketSent);

            void HandleTypedPacket(GameConnectionConduit conduit, GamePacket packet)
            {
                _sessionDispatcher.Dispatch(Unsafe.As<GameServerSession>(conduit.Connection.UserState!), packet);
            }

            listener.TeraPacketReceived += HandleTypedPacket;
            listener.ArisePacketReceived += HandleTypedPacket;

            _listeners.Enqueue(listener);
        }

        // Loading the data center and zone geometry can allocate a lot of temporary memory, so force an aggressive
        // cleanup before allowing clients to connect.
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);

        foreach (var listener in _listeners)
        {
            listener.Start();

            Log.StartedListening(_logger, listener.EndPoint);
        }
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        foreach (var listener in _listeners)
            await listener.DisposeAsync();
    }
}
