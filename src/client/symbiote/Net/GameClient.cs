using Arise.Client.Net.Sessions;
using Arise.Server.Net.Sessions;

namespace Arise.Client.Net;

[RegisterSingleton]
[SuppressMessage("", "CA1812")]
internal sealed partial class GameClient : BackgroundService
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

    private readonly IOptions<SymbioteOptions> _options;

    private readonly ILogger<GameClient> _logger;

    private readonly ObjectPoolProvider _objectPoolProvider;

    private readonly GameClientSessionDispatcher _sessionDispatcher;

    public GameClient(
        IOptions<SymbioteOptions> options,
        ILogger<GameClient> logger,
        ObjectPoolProvider objectPoolProvider,
        GameClientSessionDispatcher sessionDispatcher)
    {
        _options = options;
        _logger = logger;
        _objectPoolProvider = objectPoolProvider;
        _sessionDispatcher = sessionDispatcher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async ValueTask<string> GetManifestResourceAsync(string name)
        {
            await using var stream = typeof(ThisAssembly).Assembly.GetManifestResourceStream(name)!;

            var buffer = GC.AllocateUninitializedArray<byte>((int)stream.Length);

            await stream.ReadExactlyAsync(buffer, stoppingToken);

            return Encoding.UTF8.GetString(buffer);
        }

        var clientPem = await GetManifestResourceAsync("arise.pem");
        var clientKey = await GetManifestResourceAsync("arise.key");

        using var caCert = X509Certificate2.CreateFromPem(await GetManifestResourceAsync("ca.pem"));
        using var clientCert = X509Certificate2.CreateFromPem(clientPem, clientKey);

        await using var client = GameConnectionClient.Create(_objectPoolProvider);

        client.ConnectionEstablished += conn =>
        {
            conn.UserState = new GameClientSession(conn);

            Log.ClientConnected(_logger, conn.EndPoint);
        };

        client.ConnectionClosed += (conn, ex) => Log.ClientDisconnected(_logger, ex, conn.EndPoint);

        void LogPacket<T>(
            T code,
            ReadOnlyMemory<byte> payload,
            Action<ILogger<GameClient>, T, int> logger)
            where T : unmanaged, Enum
        {
            logger(_logger, code, payload.Length);
        }

        client.RawTeraPacketReceived += (conduit, code, payload) => LogPacket(code, payload, Log.TeraPacketReceived);
        client.RawArisePacketReceived += (conduit, code, payload) => LogPacket(code, payload, Log.ArisePacketReceived);
        client.RawTeraPacketSent += (conduit, code, payload) => LogPacket(code, payload, Log.TeraPacketSent);
        client.RawArisePacketSent += (conduit, code, payload) => LogPacket(code, payload, Log.ArisePacketSent);

        void HandleTypedPacket(GameConnectionConduit conduit, GamePacket packet)
        {
            _sessionDispatcher.Dispatch(Unsafe.As<GameClientSession>(conduit.Connection.UserState!), packet);
        }

        // TODO: Dispatch TERA packets to the game.

        client.TeraPacketReceived += HandleTypedPacket;
        client.ArisePacketReceived += HandleTypedPacket;

        var uri = _options.Value.WorldServerUri;

        _ = await client.ConnectAsync(new(uri.Host, uri.Port), caCert, clientCert, stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
