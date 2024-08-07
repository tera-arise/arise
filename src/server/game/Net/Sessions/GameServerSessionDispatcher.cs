// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Server.Net.Handlers;

namespace Arise.Server.Net.Sessions;

[RegisterSingleton<GameServerSessionDispatcher>]
internal sealed partial class GameServerSessionDispatcher :
    GameSessionDispatcher<GameServerSession, GameServerPacketHandler>
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "No game packet handler found for {Code} from {EndPoint}")]
        public static partial void NoHandlerFound(
            ILogger<GameServerSessionDispatcher> logger, GamePacketCode code, IPEndPoint endPoint);
    }

    private readonly ILogger<GameServerSessionDispatcher> _logger;

    public GameServerSessionDispatcher(ILogger<GameServerSessionDispatcher> logger)
    {
        _logger = logger;
    }

    protected override void UnhandledPacket(GameServerSession session, GamePacket packet)
    {
        Log.NoHandlerFound(_logger, packet.Code, session.EndPoint);
    }
}
