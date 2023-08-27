namespace Arise.Client;

[SuppressMessage("", "CA1812")]
internal sealed class GameApplicationHost : IHostedService
{
    private readonly Action _wake;

    public GameApplicationHost(Action wake)
    {
        _wake = wake;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        // Wake up the game now that we are fully initialized.
        _wake();

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
