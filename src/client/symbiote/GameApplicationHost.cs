namespace Arise.Client;

[SuppressMessage("", "CA1812")]
internal sealed class GameApplicationHost : BackgroundService
{
    private readonly Action _waker;

    public GameApplicationHost(Action waker)
    {
        _waker = waker;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wake up the game now that we are fully initialized.
        _waker();

        return Task.CompletedTask;
    }
}
