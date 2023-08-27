namespace Arise.Client;

[SuppressMessage("", "CA1812")]
internal sealed class GameApplicationHost : IHostedService
{
    private readonly Action _waker;

    public GameApplicationHost(Action waker)
    {
        _waker = waker;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        // Wake up the game now that we are fully initialized.
        _waker();

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
