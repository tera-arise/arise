namespace Arise.Net;

public abstract class GameConnectionManager : IAsyncDisposable
{
    protected const int CloseErrorCode = 42153;

    protected const int StreamErrorCode = 35124;

    public event Action<GameConnectionManager, GameConnection>? ClientConnected;

    public event Action<GameConnectionManager, GameConnection, Exception?>? ClientDisconnected;

    internal ObjectPool<GameConnectionBuffer> Buffers { get; }

    private readonly object _lock = new();

    private HashSet<GameConnection>? _connections = new();

    private int _disposed;

    private protected GameConnectionManager(ObjectPool<GameConnectionBuffer> buffers)
    {
        Buffers = buffers;
    }

    ~GameConnectionManager()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        await DisposeCoreAsync().ConfigureAwait(false);

        Task[] disposeTasks;

        lock (_lock)
        {
            disposeTasks = _connections!
                .Select(static conn => conn.DisposeAsync().AsTask())
                .ToArray();

            _connections = null;
        }

        await Task.WhenAll(disposeTasks).ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }

    private protected virtual ValueTask DisposeCoreAsync()
    {
        return ValueTask.CompletedTask;
    }

    protected async ValueTask<GameConnection> CreateConnectionAsync(
        QuicConnection quicConnection,
        QuicStream lowPriority,
        QuicStream normalPriority,
        QuicStream highPriority,
        ReadOnlyMemory<byte> module)
    {
        var connection = new GameConnection(this, module, quicConnection, lowPriority, normalPriority, highPriority);

        ClientConnected?.Invoke(this, connection);

        connection.Disconnected += (conn, ex) =>
        {
            // Remove the connection from _connections if this disconnection was not due to manager disposal.
            lock (_lock)
                if (_connections != null)
                    _ = _connections.Remove(conn);

            ClientDisconnected?.Invoke(this, conn, ex);
        };

        var added = false;

        lock (_lock)
        {
            if (_connections != null)
            {
                _ = _connections.Add(connection);

                added = true;
            }
        }

        // Note that this allows GameConnection.Disconnected to be invoked, so it must come after the connection is
        // added to _connections as well as the invocation of the ClientConnected event. Even if the connection was not
        // added (due to DisposeAsync having been called), we still need to call Start, or the conduit receive/send
        // tasks will be stuck.
        connection.Start();

        // Was DisposeAsync called in the meantime? If so, just drop the connection.
        if (!added)
            await connection.DisposeAsync().ConfigureAwait(false);

        return connection;
    }
}
