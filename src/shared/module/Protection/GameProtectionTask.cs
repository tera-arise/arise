namespace Arise.Module.Protection;

internal abstract class GameProtectionTask
{
    public void Start()
    {
        _ = Task.Run(async () =>
        {
            Initialize();

            while (!Environment.HasShutdownStarted)
            {
                if (!Check())
                    GameProtection.Terminate();

                await Task.Delay(GetCheckInterval()).ConfigureAwait(false);
            }
        });
    }

    protected virtual void Initialize()
    {
    }

    protected abstract bool Check();

    [SpecialName]
    private static int GetCheckInterval()
    {
        // Filled in by the server's BridgeModuleProvider.
        return 42;
    }
}
