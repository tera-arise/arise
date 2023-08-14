using Arise.Server.Bridge;

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
    }

    private readonly IOptions<WorldOptions> _options;

    private readonly ILogger<GameServer> _logger;

    private readonly BridgeModuleProvider _moduleProvider;

    private readonly ObjectPoolProvider _objectPoolProvider;

    private readonly GameSessionManager _sessionManager;

    public GameServer(
        IOptions<WorldOptions> options,
        ILogger<GameServer> logger,
        BridgeModuleProvider moduleProvider,
        ObjectPoolProvider objectPoolProvider,
        GameSessionManager sessionManager)
    {
        _options = options;
        _logger = logger;
        _moduleProvider = moduleProvider;
        _objectPoolProvider = objectPoolProvider;
        _sessionManager = sessionManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        static string GetManifestResource(string name)
        {
            using var stream = typeof(ThisAssembly).Assembly.GetManifestResourceStream(name)!;

            var buffer = new byte[stream.Length];

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
                _sessionManager.AddSession(conn);

                Log.ClientConnected(_logger, conn.EndPoint);
            };

            listener.ConnectionDropped += (_, ep, ex) =>
            {
                if (ep != null)
                    Log.ClientDropped(_logger, ex, ep);
                else
                    Log.ClientDropped(_logger, ex);
            };

            listener.ConnectionClosed += (conn, ex) =>
            {
                _sessionManager.RemoveSession(conn);

                Log.ClientDisconnected(_logger, ex, conn.EndPoint);
            };

            // TODO: Handle packets.

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
