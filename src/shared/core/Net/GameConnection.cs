using Arise.Bridge;
using Arise.Net.Packets;

namespace Arise.Net;

public sealed class GameConnection : IAsyncDisposable
{
    public event Action<GameConnection, GameConnectionException?>? Disconnected;

    public event Action<GameConnectionConduit, TeraGamePacketCode, ReadOnlyMemory<byte>>? RawTeraPacketReceived;

    public event Action<GameConnectionConduit, AriseGamePacketCode, ReadOnlyMemory<byte>>? RawArisePacketReceived;

    public event Action<GameConnectionConduit, TeraGamePacket>? TeraPacketReceived;

    public event Action<GameConnectionConduit, AriseGamePacket>? ArisePacketReceived;

    public GameConnectionManager Manager { get; }

    public IPEndPoint EndPoint => _connection.RemoteEndPoint;

    public BridgeModule Module => _module.Instance;

    public GameConnectionConduit LowPriority { get; }

    public GameConnectionConduit NormalPriority { get; }

    public GameConnectionConduit HighPriority { get; }

    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly GameConnectionModule _module;

    private readonly QuicConnection _connection;

    private int _disposed;

    internal GameConnection(
        GameConnectionManager manager,
        ReadOnlyMemory<byte> module,
        QuicConnection connection,
        QuicStream lowPriority,
        QuicStream normalPriority,
        QuicStream highPriority)
    {
        Manager = manager;
        _module = new(module);
        _connection = connection;
        LowPriority = new(this, lowPriority, _ready.Task);
        NormalPriority = new(this, normalPriority, _ready.Task);
        HighPriority = new(this, highPriority, _ready.Task);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        var exception = default(GameConnectionException);

        try
        {
            await Task
                .WhenAll(
                    LowPriority.DisposeAsync().AsTask(),
                    NormalPriority.DisposeAsync().AsTask(),
                    HighPriority.DisposeAsync().AsTask())
                .PreserveAggregateException()
                .ConfigureAwait(false);
        }
        catch (AggregateException ex)
        {
            var flattened = ex.Flatten();

            // Can only be genuine network errors (IsNetworkException and not connection/stream abort).
            exception = new(
                "A game connection was disconnected due to an error.",
                flattened.InnerExceptions.Count == 1 ? flattened.InnerException! : flattened);
        }

        _module.Dispose();

        await _connection.DisposeAsync().ConfigureAwait(false);

        Disconnected?.Invoke(this, exception);
    }

    internal void InternalDispose()
    {
        // Used by GameConnectionConduit when the connection has somehow broken down and the GameConnection needs to be
        // disposed. Since DisposeAsync awaits GameConnectionConduit.DisposeAsync, this has to be dispatched to the
        // thread pool or we would deadlock.
        //
        // The _disposed check here is inherently racey but might prevent creation of an unnecessary task.
        if (_disposed == 0)
            _ = Task.Run(async () => await DisposeAsync().ConfigureAwait(false));
    }

    public void Start()
    {
        _ready.SetResult();
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

    internal static bool IsNetworkException(Exception exception)
    {
        return exception is SocketException or AuthenticationException or QuicException;
    }

    internal static bool IsInnocuousError(QuicError error)
    {
        // Normal errors that do not indicate an actual network problem.
        return error is QuicError.ConnectionAborted or QuicError.StreamAborted or QuicError.ConnectionIdle;
    }
}
