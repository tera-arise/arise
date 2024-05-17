// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Threading;

internal sealed class DispatcherSynchronizationContext : SynchronizationContext
{
    private readonly Dispatcher _dispatcher;

    public DispatcherSynchronizationContext(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        // It seems that the norm for SynchronizationContext implementations is to just fail silently if posting the
        // callback for execution is unsuccessful. This is probably fine anyway, since this method will only really be
        // called by an await operation from within the dispatcher, implying that it is still alive.
        _ = _dispatcher.Post(d, state);
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        if (_dispatcher.IsCurrent)
            d(state);
        else
            Post(d, state);
    }

    public override SynchronizationContext CreateCopy()
    {
        // No need to copy the context as we have no mutable state.
        return this;
    }
}
