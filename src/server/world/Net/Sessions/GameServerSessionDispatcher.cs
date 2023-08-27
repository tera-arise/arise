using Arise.Server.Net.Handlers;

namespace Arise.Server.Net.Sessions;

[RegisterSingleton<GameServerSessionDispatcher>]
[SuppressMessage("", "CA1812")]
internal sealed partial class GameServerSessionDispatcher :
    GameSessionDispatcher<GameServerSession, GameServerPacketHandler>
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "No game packet handler found for {Channel}:{Code} from {EndPoint}")]
        public static partial void NoHandlerFound(
            ILogger<GameServerSessionDispatcher> logger, GameConnectionChannel channel, Enum code, IPEndPoint endPoint);
    }

    private readonly ILogger<GameServerSessionDispatcher> _logger;

    public GameServerSessionDispatcher(ILogger<GameServerSessionDispatcher> logger)
    {
        _logger = logger;
    }

    protected override void UnhandledPacket(GameServerSession session, GamePacket packet)
    {
        Log.NoHandlerFound(
            _logger,
            packet.Channel,
            packet.Channel switch
            {
                GameConnectionChannel.Tera => (TeraGamePacketCode)packet.RawCode,
                GameConnectionChannel.Arise => (AriseGamePacketCode)packet.RawCode,
                _ => throw new UnreachableException(),
            },
            session.EndPoint);
    }
}
