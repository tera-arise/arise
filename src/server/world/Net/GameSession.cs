namespace Arise.Server.Net;

internal sealed class GameSession
{
    // TODO: Add important state (AccountDocument, Player, etc).

    public GameSessionPort LowPriority { get; }

    public GameSessionPort NormalPriority { get; }

    public GameSessionPort HighPriority { get; }

    private readonly GameConnection _connection;

    public GameSession(GameConnection connection)
    {
        _connection = connection;
        LowPriority = new(connection.LowPriority);
        NormalPriority = new(connection.NormalPriority);
        HighPriority = new(connection.HighPriority);
    }

    public ValueTask DisconnectAsync()
    {
        return _connection.DisposeAsync();
    }
}
