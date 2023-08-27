using Arise.Client.Net.Sessions;
using Arise.Server.Net.Sessions;

namespace Arise.Client.Net;

[RegisterSingleton]
[SuppressMessage("", "CA1812")]
internal sealed partial class GameClient : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Game client connected to {EndPoint}")]
        public static partial void ClientConnected(ILogger<GameClient> logger, IPEndPoint endPoint);

        [LoggerMessage(1, LogLevel.Information, "Game client disconnected from {EndPoint}")]
        public static partial void ClientDisconnected(
            ILogger<GameClient> logger, Exception? exception, IPEndPoint endPoint);

        // TODO: https://github.com/dotnet/runtime/issues/90589

        [LoggerMessage(2, LogLevel.Trace, "S -> C: Tera:{Code} ({Length} bytes)")]
        public static partial void TeraPacketReceived(ILogger<GameClient> logger, TeraGamePacketCode code, int length);

        [LoggerMessage(3, LogLevel.Trace, "S -> C: Arise:{Code} ({Length} bytes)")]
        public static partial void ArisePacketReceived(
            ILogger<GameClient> logger, AriseGamePacketCode code, int length);

        [LoggerMessage(4, LogLevel.Trace, "C -> S: Tera:{Code} ({Length} bytes)")]
        public static partial void TeraPacketSent(ILogger<GameClient> logger, TeraGamePacketCode code, int length);

        [LoggerMessage(5, LogLevel.Trace, "C -> S: Arise:{Code} ({Length} bytes)")]
        public static partial void ArisePacketSent(ILogger<GameClient> logger, AriseGamePacketCode code, int length);
    }

    public GameClientSession Session { get; private set; } = null!;

    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly IOptions<SymbioteOptions> _options;

    private readonly ILogger<GameClient> _logger;

    private readonly GameClientSessionDispatcher _sessionDispatcher;

    private readonly TeraConnectionManager _connectionManager;

    private readonly GameConnectionClient _client;

    public GameClient(
        IHostApplicationLifetime hostLifetime,
        IOptions<SymbioteOptions> options,
        ILogger<GameClient> logger,
        ObjectPoolProvider objectPoolProvider,
        GameClientSessionDispatcher sessionDispatcher,
        TeraConnectionManager connectionManager)
    {
        _hostLifetime = hostLifetime;
        _options = options;
        _logger = logger;
        _sessionDispatcher = sessionDispatcher;
        _connectionManager = connectionManager;
        _client = GameConnectionClient.Create(objectPoolProvider);
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _client.ConnectionEstablished += conn =>
        {
            conn.UserState = Session = new GameClientSession(conn);

            _connectionManager.Disconnected += () => conn.DisposeAsync().AsTask().GetAwaiter().GetResult();

            // TODO: Use packet prioritization here - maybe a table of low/high priority codes.
            _connectionManager.PacketSent += (payload, code) => conn.NormalPriority.PostPacket(code, payload);

            Log.ClientConnected(_logger, conn.EndPoint);
        };

        _client.ConnectionClosed += (conn, ex) =>
        {
            // If something unexpected broke the connection, notify the game so it can act accordingly.
            _connectionManager.Disconnect();

            Log.ClientDisconnected(_logger, ex, conn.EndPoint);

            // One way or another, a disconnection will cause the client to exit, so we should do the same.
            _hostLifetime.StopApplication();
        };

        void LogPacket<T>(T code, ReadOnlyMemory<byte> payload, Action<ILogger<GameClient>, T, int> logger)
            where T : unmanaged, Enum
        {
            logger(_logger, code, payload.Length);
        }

        _client.RawTeraPacketReceived += (conduit, code, payload) =>
        {
            _connectionManager.EnqueuePacket(code, payload.Span);

            LogPacket(code, payload, Log.TeraPacketReceived);
        };
        _client.RawArisePacketReceived += (conduit, code, payload) => LogPacket(code, payload, Log.ArisePacketReceived);
        _client.RawTeraPacketSent += (conduit, code, payload) => LogPacket(code, payload, Log.TeraPacketSent);
        _client.RawArisePacketSent += (conduit, code, payload) => LogPacket(code, payload, Log.ArisePacketSent);

        _client.ArisePacketReceived +=
            (conduit, packet) =>
                _sessionDispatcher.Dispatch(Unsafe.As<GameClientSession>(conduit.Connection.UserState!), packet);

        async ValueTask<string> GetManifestResourceAsync(string name)
        {
            await using var stream = typeof(ThisAssembly).Assembly.GetManifestResourceStream(name)!;

            var buffer = GC.AllocateUninitializedArray<byte>((int)stream.Length);

            await stream.ReadExactlyAsync(buffer, cancellationToken);

            return Encoding.UTF8.GetString(buffer);
        }

        var clientPem = await GetManifestResourceAsync("arise.pem");
        var clientKey = await GetManifestResourceAsync("arise.key");

        using var caCert = X509Certificate2.CreateFromPem(await GetManifestResourceAsync("ca.pem"));
        using var clientCert = X509Certificate2.CreateFromPem(clientPem, clientKey);

        var uri = _options.Value.WorldServerUri;

        _ = await _client.ConnectAsync(new(uri.Host, uri.Port), caCert, clientCert, cancellationToken);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return _client.DisposeAsync().AsTask();
    }
}
