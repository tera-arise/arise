namespace Arise.Server.Net.Sessions;

internal sealed class GameServerSession : GameSession
{
    // TODO: Add important state (AccountDocument, Player, etc).

    public GameServerSession(GameConnection connection)
        : base(connection)
    {
    }
}
