using Arise.Client.Game.Net.Sessions;

namespace Arise.Client.Game.Net;

internal sealed partial class GameClient : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Game client connected to {EndPoint}")]
        public static partial void ClientConnected(ILogger<GameClient> logger, IPEndPoint endPoint);

        [LoggerMessage(1, LogLevel.Information, "Game client disconnected from {EndPoint}")]
        public static partial void ClientDisconnected(
            ILogger<GameClient> logger, Exception? exception, IPEndPoint endPoint);

        [LoggerMessage(2, LogLevel.Trace, "S -> C: {Code} ({Length} bytes)")]
        public static partial void PacketReceived(ILogger<GameClient> logger, GamePacketCode code, int length);

        [LoggerMessage(3, LogLevel.Trace, "C -> S: {Code} ({Length} bytes)")]
        public static partial void PacketSent(ILogger<GameClient> logger, GamePacketCode code, int length);
    }

    public GameClientSession Session { get; private set; } = null!;

    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly IOptions<GameOptions> _options;

    private readonly ILogger<GameClient> _logger;

    private readonly GameClientSessionDispatcher _sessionDispatcher;

    private readonly TeraConnectionManager _connectionManager;

    private readonly GameConnectionClient _client;

    public GameClient(
        IHostApplicationLifetime hostLifetime,
        IOptions<GameOptions> options,
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

        void LogPacket(
            GamePacketCode code,
            ReadOnlyMemory<byte> payload,
            Action<ILogger<GameClient>, GamePacketCode, int> log)
        {
            log(_logger, code, payload.Length);
        }

        _client.RawPacketReceived += (conduit, code, payload) =>
        {
            _connectionManager.EnqueuePacket(code, payload.Span);

            LogPacket(code, payload, Log.PacketReceived);
        };
        _client.RawPacketSent += (conduit, code, payload) => LogPacket(code, payload, Log.PacketSent);

        _client.ArisePacketReceived +=
            (conduit, packet) =>
                _sessionDispatcher.Dispatch(Unsafe.As<GameClientSession>(conduit.Connection.UserState!), packet);

        async ValueTask<string> GetManifestResourceAsync(string name)
        {
            var stream = typeof(ThisAssembly).Assembly.GetManifestResourceStream(name)!;

            await using (stream.ConfigureAwait(false))
            {
                var buffer = GC.AllocateUninitializedArray<byte>((int)stream.Length);

                await stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);

                return Encoding.UTF8.GetString(buffer);
            }
        }

        var clientPem = await GetManifestResourceAsync("arise.pem").ConfigureAwait(false);
        var clientKey = await GetManifestResourceAsync("arise.key").ConfigureAwait(false);

        using var caCert = X509Certificate2.CreateFromPem(
            await GetManifestResourceAsync("ca.pem").ConfigureAwait(false));
        using var clientCert = X509Certificate2.CreateFromPem(clientPem, clientKey);

        var uri = _options.Value.GameServerUri;

        _ = await _client.ConnectAsync(new(uri.Host, uri.Port), caCert, clientCert, cancellationToken)
            .ConfigureAwait(false);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return _client.DisposeAsync().AsTask();
    }
}
