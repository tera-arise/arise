namespace Arise.Net.Sessions;

public abstract class GameSession
{
    public IPEndPoint EndPoint => _connection.EndPoint;

    public GameSessionPort LowPriority { get; }

    public GameSessionPort NormalPriority { get; }

    public GameSessionPort HighPriority { get; }

    private readonly GameConnection _connection;

    protected GameSession(GameConnection connection)
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
