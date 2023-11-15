using Arise.Bridge;
using Arise.Net.Packets;
using Arise.Net.Serialization;

namespace Arise.Net;

public abstract class GameConnectionManager : IAsyncDisposable
{
    protected const int CloseErrorCode = 42153;

    protected const int StreamErrorCode = 35124;

    // Connection notification events may not throw, and may not call GameConnection.DisposeAsync.

    public event Action<GameConnection>? ConnectionEstablished;

    public event Action<GameConnection, GameConnectionException?>? ConnectionClosed;

    // Packet notification events may only throw InvalidDataException.

    public event Action<GameConnectionConduit, TeraGamePacketCode, ReadOnlyMemory<byte>>? RawTeraPacketReceived;

    public event Action<GameConnectionConduit, AriseGamePacketCode, ReadOnlyMemory<byte>>? RawArisePacketReceived;

    public event Action<GameConnectionConduit, TeraGamePacket>? TeraPacketReceived;

    public event Action<GameConnectionConduit, AriseGamePacket>? ArisePacketReceived;

    public event Action<GameConnectionConduit, TeraGamePacketCode, ReadOnlyMemory<byte>>? RawTeraPacketSent;

    public event Action<GameConnectionConduit, AriseGamePacketCode, ReadOnlyMemory<byte>>? RawArisePacketSent;

    public event Action<GameConnectionConduit, TeraGamePacket>? TeraPacketSent;

    public event Action<GameConnectionConduit, AriseGamePacket>? ArisePacketSent;

    internal ObjectPool<GameConnectionBuffer> Buffers { get; }

    private readonly object _lock = new();

    private HashSet<GameConnection>? _connections = [];

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

    internal void HandleReceivedPacket(GameConnectionConduit conduit, GameConnectionBuffer buffer)
    {
        HandlePacket(
            conduit, buffer, RawTeraPacketReceived, RawArisePacketReceived, TeraPacketReceived, ArisePacketReceived);
    }

    internal void HandleSentPacket(GameConnectionConduit conduit, GameConnectionBuffer buffer)
    {
        HandlePacket(conduit, buffer, RawTeraPacketSent, RawArisePacketSent, TeraPacketSent, ArisePacketSent);
    }

    private static void HandlePacket(
        GameConnectionConduit conduit,
        GameConnectionBuffer buffer,
        Action<GameConnectionConduit, TeraGamePacketCode, ReadOnlyMemory<byte>>? rawTeraEvent,
        Action<GameConnectionConduit, AriseGamePacketCode, ReadOnlyMemory<byte>>? rawAriseEvent,
        Action<GameConnectionConduit, TeraGamePacket>? teraEvent,
        Action<GameConnectionConduit, AriseGamePacket>? ariseEvent)
    {
        void RaisePacketEvents<TCode, TPacket>(
            GamePacketSerializer<TCode, TPacket> serializer,
            Action<GameConnectionConduit, TCode, ReadOnlyMemory<byte>>? rawEvent,
            Action<GameConnectionConduit, TPacket>? @event)
            where TCode : unmanaged, Enum
            where TPacket : GamePacket<TCode>
        {
            var code = Unsafe.BitCast<ushort, TCode>(buffer.Code);

            rawEvent?.Invoke(conduit, code, buffer.Payload);

            if (@event == null || serializer.CreatePacket(code) is not { } packet)
                return;

            buffer.ResetStream(length: buffer.Length);

            serializer.DeserializePacket(packet, buffer.PayloadAccessor);

            @event.Invoke(conduit, packet);
        }

        switch (buffer.Channel)
        {
            case GameConnectionChannel.Tera:
                RaisePacketEvents(TeraGamePacketSerializer.Instance, rawTeraEvent, teraEvent);
                break;
            case GameConnectionChannel.Arise:
                RaisePacketEvents(AriseGamePacketSerializer.Instance, rawAriseEvent, ariseEvent);
                break;
        }
    }

    private protected async ValueTask<GameConnection> CreateConnectionAsync(
        QuicConnection quicConnection,
        QuicStream lowPriority,
        QuicStream normalPriority,
        QuicStream highPriority,
        BridgeModule module)
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
