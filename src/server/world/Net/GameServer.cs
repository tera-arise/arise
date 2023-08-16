using Arise.Server.Bridge;
using Arise.Server.Net.Handlers;

namespace Arise.Server.Net;

[SuppressMessage("", "CA1812")]
internal sealed partial class GameServer : BackgroundService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Game server now listening on: {endPoint}")]
        public static partial void StartedListening(ILogger logger, IPEndPoint endPoint);

        [LoggerMessage(1, LogLevel.Information, "Game client connected from {endPoint}")]
        public static partial void ClientConnected(ILogger logger, IPEndPoint endPoint);

        [LoggerMessage(2, LogLevel.Debug, "Game client dropped")]
        public static partial void ClientDropped(ILogger logger, Exception? exception);

        [LoggerMessage(3, LogLevel.Debug, "Game client from {endPoint} dropped")]
        public static partial void ClientDropped(ILogger logger, Exception? exception, IPEndPoint endPoint);

        [LoggerMessage(4, LogLevel.Information, "Game client from {endPoint} disconnected")]
        public static partial void ClientDisconnected(ILogger logger, Exception? exception, IPEndPoint endPoint);

        // TODO: https://github.com/dotnet/runtime/issues/90589

        [LoggerMessage(5, LogLevel.Trace, "C -> S {EndPoint}: Tera:{Code} ({Length} bytes)")]
        public static partial void TeraPacketReceived(
            ILogger logger, IPEndPoint endPoint, TeraGamePacketCode code, int length);

        [LoggerMessage(6, LogLevel.Trace, "C -> S {EndPoint}: Arise:{Code} ({Length} bytes)")]
        public static partial void ArisePacketReceived(
            ILogger logger, IPEndPoint endPoint, AriseGamePacketCode code, int length);

        [LoggerMessage(7, LogLevel.Trace, "S -> C {EndPoint}: Tera:{Code} ({Length} bytes)")]
        public static partial void TeraPacketSent(
            ILogger logger, IPEndPoint endPoint, TeraGamePacketCode code, int length);

        [LoggerMessage(8, LogLevel.Trace, "S -> C {EndPoint}: Arise:{Code} ({Length} bytes)")]
        public static partial void ArisePacketSent(
            ILogger logger, IPEndPoint endPoint, AriseGamePacketCode code, int length);
    }

    private readonly IOptions<WorldOptions> _options;

    private readonly ILogger<GameServer> _logger;

    private readonly BridgeModuleProvider _moduleProvider;

    private readonly ObjectPoolProvider _objectPoolProvider;

    private readonly GamePacketHandler _packetHandler;

    public GameServer(
        IOptions<WorldOptions> options,
        ILogger<GameServer> logger,
        BridgeModuleProvider moduleProvider,
        ObjectPoolProvider objectPoolProvider,
        GamePacketHandler packetHandler)
    {
        _options = options;
        _logger = logger;
        _moduleProvider = moduleProvider;
        _objectPoolProvider = objectPoolProvider;
        _packetHandler = packetHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        static string GetManifestResource(string name)
        {
            using var stream = typeof(ThisAssembly).Assembly.GetManifestResourceStream(name)!;

            var buffer = GC.AllocateUninitializedArray<byte>((int)stream.Length);

            // Manifest resources come from memory; async/await would be pointless here.
            stream.ReadExactly(buffer);

            return Encoding.UTF8.GetString(buffer);
        }

        using var caCert = X509Certificate2.CreateFromPem(GetManifestResource("ca.pem"));
        using var serverCert = X509Certificate2.CreateFromPem(
            GetManifestResource("arised.pem"), GetManifestResource("arised.key"));

        var listeners = new List<GameConnectionListener>();

        foreach (var ep in _options.Value.Endpoints)
        {
            var listener = await GameConnectionListener.CreateAsync(
                IPEndPoint.Parse(ep),
                caCert,
                serverCert,
                _moduleProvider.GetRandomModulePair,
                _objectPoolProvider,
                stoppingToken);

            listener.ConnectionEstablished += conn =>
            {
                conn.UserState = new GameSession(conn);

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
                Action<ILogger, IPEndPoint, T, int> logger)
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
                _packetHandler.Dispatch(Unsafe.As<GameSession>(conduit.Connection.UserState!), packet);
            }

            listener.TeraPacketReceived += HandleTypedPacket;
            listener.ArisePacketReceived += HandleTypedPacket;

            listeners.Add(listener);
        }

        foreach (var listener in listeners)
        {
            listener.Start();

            Log.StartedListening(_logger, listener.EndPoint);
        }

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        finally
        {
            foreach (var listener in listeners)
                await listener.DisposeAsync();
        }
    }
}
