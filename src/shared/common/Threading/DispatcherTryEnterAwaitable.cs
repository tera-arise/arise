// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Threading;

[SuppressMessage("", "CA1815")]
public readonly struct DispatcherTryEnterAwaitable
{
    public readonly struct DispatcherEnterAwaiter : ICriticalNotifyCompletion
    {
        public bool IsCompleted => _dispatcher.IsCurrent;

        private readonly Dispatcher _dispatcher;

        internal DispatcherEnterAwaiter(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void OnCompleted(Action continuation)
        {
            // The compiler will prefer UnsafeOnCompleted, so this method is essentially never used. Boxing a tuple here
            // is of little concern.
            OnCompleted(
                static state =>
                {
                    var (execCtx, callback) = Unsafe.Unbox<(ExecutionContext, Action)>(state!);

                    ExecutionContext.Restore(execCtx);

                    callback();
                },
                (ExecutionContext.Capture(), continuation));
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(static state => Unsafe.As<Action>(state!)(), continuation);
        }

        private void OnCompleted(SendOrPostCallback callback, object? state)
        {
            // This method is only called if IsCompleted was false initially, i.e. we were not on the target dispatcher.
            if (!_dispatcher.Post(callback, state))
                callback(state);
        }

        public bool GetResult()
        {
            // This method is only intended to be invoked by immediately awaiting a DispatcherEnterAwaitable exactly
            // once. Any other way of calling this method is erroneous and can give a wrong result.
            return IsCompleted;
        }
    }

    private readonly Dispatcher _dispatcher;

    internal DispatcherTryEnterAwaitable(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public DispatcherEnterAwaiter GetAwaiter()
    {
        return new(_dispatcher);
    }
}
