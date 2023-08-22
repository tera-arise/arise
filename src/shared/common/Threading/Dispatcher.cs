namespace Arise.Threading;

public sealed class Dispatcher
{
    public static Dispatcher? Current => _current;

    public bool IsAlive => _work.Reader.Completion.IsCompleted;

    public bool IsCurrent => this == _current;

    [ThreadStatic]
    private static Dispatcher? _current;

    private readonly Channel<(SendOrPostCallback, object?)> _work =
        Channel.CreateUnbounded<(SendOrPostCallback, object?)>(new()
        {
            SingleReader = true,
        });

    public Dispatcher()
    {
        _ = Task.Run(async () =>
        {
            var syncCtx = new DispatcherSynchronizationContext(this);

            await foreach (var (callback, state) in _work.Reader.ReadAllAsync())
            {
                // This ensures that any await operation in the callback will yield back to this dispatcher, in a
                // similar fashion to how await works in a UI framework.
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                _current = this;

                try
                {
                    callback(state);
                }
                finally
                {
                    _current = null;
                }

                // No need to clean up SynchronizationContext/ExecutionContext; thread pool threads reset these back to
                // their defaults after processing a work item.
            }
        });
    }

    public async ValueTask ExitAsync()
    {
        _work.Writer.Complete();

        // Ensure that the dispatcher immediately exits when the caller awaits the returned ValueTask.
        await Task.Yield();
    }

    internal bool Post(SendOrPostCallback callback, object? state)
    {
        return _work.Writer.TryWrite((callback, state));
    }

    public DispatcherTryEnterAwaitable TryEnter()
    {
        // Like ValueTask, this should be immediately awaited exactly once.
        return new(this);
    }
}
