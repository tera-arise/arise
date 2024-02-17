using Arise.Client.Game.Net.Handlers;

namespace Arise.Client.Game.Net.Sessions;

internal sealed partial class GameClientSessionDispatcher :
    GameSessionDispatcher<GameClientSession, GameClientPacketHandler>
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "No game packet handler found for {Code} from {EndPoint}")]
        public static partial void NoHandlerFound(
            ILogger<GameClientSessionDispatcher> logger, GamePacketCode code, IPEndPoint endPoint);
    }

    private readonly ILogger<GameClientSessionDispatcher> _logger;

    public GameClientSessionDispatcher(ILogger<GameClientSessionDispatcher> logger)
    {
        _logger = logger;
    }

    protected override void UnhandledPacket(GameClientSession session, GamePacket packet)
    {
        Log.NoHandlerFound(_logger, packet.Code, session.EndPoint);
    }
}
