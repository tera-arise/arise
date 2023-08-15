using Arise.Bridge;

namespace Arise.Net;

public sealed class GameConnection : IAsyncDisposable
{
    public GameConnectionManager Manager { get; }

    public IPEndPoint EndPoint => _connection.RemoteEndPoint;

    public BridgeModule Module { get; }

    public GameConnectionConduit LowPriority { get; }

    public GameConnectionConduit NormalPriority { get; }

    public GameConnectionConduit HighPriority { get; }

    public object? UserState { get; set; }

    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly QuicConnection _connection;

    private int _disposed;

    internal GameConnection(
        GameConnectionManager manager,
        BridgeModule module,
        QuicConnection connection,
        QuicStream lowPriority,
        QuicStream normalPriority,
        QuicStream highPriority)
    {
        Manager = manager;
        Module = module;
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
            var fex = ex.Flatten();

            // Can only be genuine network errors (IsNetworkException and not IsInnocuousError).
            exception = new(
                "A game connection was disconnected due to an error.",
                fex.InnerExceptions.Count == 1 ? fex.InnerException! : fex);
        }

        await _connection.DisposeAsync().ConfigureAwait(false);

        Manager.HandleDisconnect(this, exception);
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

    internal void Start()
    {
        _ready.SetResult();
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
