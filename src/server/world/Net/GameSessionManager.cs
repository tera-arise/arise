using System.Collections.Concurrent;

namespace Arise.Server.Net;

[SuppressMessage("", "CA1812")]
internal sealed class GameSessionManager
{
    // TODO: It would be better if we could just store a GameSession object directly on GameConnection.

    private readonly ConcurrentDictionary<GameConnection, GameSession> _sessions = new();

    public void AddSession(GameConnection connection)
    {
        _ = _sessions.TryAdd(connection, new(connection));
    }

    public void RemoveSession(GameConnection connection)
    {
        _ = _sessions.TryRemove(connection, out _);
    }

    public GameSession? GetSession(GameConnection connection)
    {
        return _sessions.GetValueOrDefault(connection);
    }
}
