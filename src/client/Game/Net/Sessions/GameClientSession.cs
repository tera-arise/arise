namespace Arise.Client.Game.Net.Sessions;

internal sealed class GameClientSession : GameSession
{
    public GameClientSession(GameConnection connection)
        : base(connection)
    {
    }
}
