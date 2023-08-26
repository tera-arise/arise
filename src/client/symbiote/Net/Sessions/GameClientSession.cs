namespace Arise.Client.Net.Sessions;

internal sealed class GameClientSession : GameSession
{
    public GameClientSession(GameConnection connection)
        : base(connection)
    {
    }
}
