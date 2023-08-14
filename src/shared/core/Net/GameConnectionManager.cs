using Arise.Net.Packets;

namespace Arise.Net;

public abstract class GameConnectionManager : IAsyncDisposable
{
    protected const int CloseErrorCode = 42153;

    protected const int StreamErrorCode = 35124;

    // Connection notification events may not throw, and may not call GameConnection.DisposeAsync.

    public event Action<GameConnection>? ConnectionEstablished;

    public event Action<GameConnection, GameConnectionException?>? ConnectionClosed;

    // Packet receipt events may only throw InvalidDataException.

    public event Action<GameConnectionConduit, TeraGamePacketCode, ReadOnlyMemory<byte>>? RawTeraPacketReceived;

    public event Action<GameConnectionConduit, AriseGamePacketCode, ReadOnlyMemory<byte>>? RawArisePacketReceived;

    public event Action<GameConnectionConduit, TeraGamePacket>? TeraPacketReceived;

    public event Action<GameConnectionConduit, AriseGamePacket>? ArisePacketReceived;

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

    internal void HandleDisconnect(GameConnection connection, GameConnectionException? exception)
    {
        // Remove the connection from _connections if this disconnection was not due to manager disposal.
        lock (_lock)
            if (_connections != null)
                _ = _connections.Remove(connection);

        ConnectionClosed?.Invoke(connection, exception);
    }

    internal void HandlePacket(GameConnectionConduit conduit, GameConnectionBuffer buffer)
    {
        var channel = buffer.Channel;
        var code = buffer.Code;

        GamePacket DeserializePacket()
        {
            var packet = GamePacketSerializer.CreatePacket(channel, code);

            buffer.ResetStream(length: buffer.Length);

            GamePacketSerializer.DeserializePacket(packet, buffer.PayloadAccessor);

            return packet;
        }

        switch (channel)
        {
            case GameConnectionChannel.Tera:
                RawTeraPacketReceived?.Invoke(conduit, (TeraGamePacketCode)code, buffer.Payload);

                if (TeraPacketReceived is { } teraReceived)
                    teraReceived(conduit, Unsafe.As<TeraGamePacket>(DeserializePacket()));

                break;
            case GameConnectionChannel.Arise:
                RawArisePacketReceived?.Invoke(conduit, (AriseGamePacketCode)code, buffer.Payload);

                if (ArisePacketReceived is { } ariseReceived)
                    ariseReceived(conduit, Unsafe.As<AriseGamePacket>(DeserializePacket()));

                break;
        }
    }

    private protected async ValueTask<GameConnection> CreateConnectionAsync(
        QuicConnection quicConnection,
        QuicStream lowPriority,
        QuicStream normalPriority,
        QuicStream highPriority,
        ReadOnlyMemory<byte> module)
    {
        var connection = new GameConnection(this, module, quicConnection, lowPriority, normalPriority, highPriority);

        var added = false;

        lock (_lock)
        {
            if (_connections != null)
            {
                _ = _connections.Add(connection);

                added = true;
            }
        }

        ConnectionEstablished?.Invoke(connection);

        // Note that this allows RemoveConnection to be invoked from the conduits, so it must come after the connection
        // is added to _connections as well as the invocation of the ConnectionEstablished event.
        //
        // Even if the connection was not added (due to this manager being disposed), we still need to call Start, or
        // the conduit receive/send tasks will be stuck in the GameConnection.DisposeAsync call below.
        connection.Start();

        // Was this manager disposed in the meantime? If so, just drop the connection.
        if (!added)
            await connection.DisposeAsync().ConfigureAwait(false);

        return connection;
    }
}
