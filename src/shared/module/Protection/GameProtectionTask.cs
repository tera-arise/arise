namespace Arise.Bridge.Protection;

[SuppressMessage("", "CA1001")]
internal abstract class GameProtectionTask
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly CancellationTokenSource _cts = new();

    private readonly TaskCompletionSource _checkDone = new(TaskCreationOptions.RunContinuationsAsynchronously);

    protected GameProtectionTask()
    {
        var ct = _cts.Token;

        _ = Task.Run(() => PerformCheckAsync(ct), ct);
    }

    public void Start()
    {
        _ = _ready.TrySetResult();
    }

    public void Stop()
    {
        // Signal the check task to shut down.
        _cts.Cancel();

        // Note that the check task is not expected to encounter any exceptions.
        _checkDone.Task.GetAwaiter().GetResult();

        // The task is gone; safe to dispose this now.
        _cts.Dispose();
    }

    protected virtual void Initialize()
    {
    }

    protected abstract bool Check();

    [Obfuscation]
    private static int GetCheckInterval()
    {
        // Filled in by the server's BridgeModuleGenerator.
        return 42;
    }

    private async Task PerformCheckAsync(CancellationToken cancellationToken)
    {
        Initialize();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!Check())
                GameProtection.Terminate();

            await Task.Delay(GetCheckInterval(), cancellationToken).ConfigureAwait(false);
        }
    }
}
