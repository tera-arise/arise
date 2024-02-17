using Arise.Server.Bridge;
using Arise.Server.Net.Sessions;

namespace Arise.Server.Net;

[RegisterSingleton<GameServer>]
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

        [LoggerMessage(5, LogLevel.Trace, "C -> S {EndPoint}: {Code} ({Length} bytes)")]
        public static partial void PacketReceived(
            ILogger<GameServer> logger, IPEndPoint endPoint, GamePacketCode code, int length);

        [LoggerMessage(6, LogLevel.Trace, "S -> C {EndPoint}: {Code} ({Length} bytes)")]
        public static partial void PacketSent(
            ILogger<GameServer> logger, IPEndPoint endPoint, GamePacketCode code, int length);
    }

    private readonly Queue<GameConnectionListener> _listeners = new();

    private readonly IOptions<GameOptions> _options;

    private readonly ILogger<GameServer> _logger;

    private readonly BridgeModuleGenerator _moduleGenerator;

    private readonly ObjectPoolProvider _objectPoolProvider;

    private readonly GameServerSessionDispatcher _sessionDispatcher;

    public GameServer(
        IOptions<GameOptions> options,
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

            void LogPacket(
                GameConnectionConduit conduit,
                GamePacketCode code,
                ReadOnlyMemory<byte> payload,
                Action<ILogger<GameServer>, IPEndPoint, GamePacketCode, int> log)
            {
                log(_logger, conduit.Connection.EndPoint, code, payload.Length);
            }

            listener.RawPacketReceived += (conduit, code, payload) =>
                LogPacket(conduit, code, payload, Log.PacketReceived);
            listener.RawPacketSent += (conduit, code, payload) =>
                LogPacket(conduit, code, payload, Log.PacketSent);

            void HandlePacket(GameConnectionConduit conduit, GamePacket packet)
            {
                _sessionDispatcher.Dispatch(Unsafe.As<GameServerSession>(conduit.Connection.UserState!), packet);
            }

            listener.TeraPacketReceived += HandlePacket;
            listener.ArisePacketReceived += HandlePacket;

            _listeners.Enqueue(listener);
        }

        // Loading data during startup can allocate a lot of temporary memory, so force an aggressive cleanup before
        // allowing clients to connect.
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
