namespace Arise.Bridge.Protection;

internal abstract class GameProtectionTask
{
    private static volatile bool _unloading;

    static GameProtectionTask()
    {
        // Exit when GameConnectionModule tries to unload us.
        AssemblyLoadContext.GetLoadContext(typeof(ThisAssembly).Assembly)!.Unloading += static _ => _unloading = true;
    }

    public void Start()
    {
        _ = Task.Run(async () =>
        {
            Initialize();

            while (!_unloading)
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

    [Obfuscation]
    private static int GetCheckInterval()
    {
        // Filled in by the server's BridgeModuleProvider.
        return 42;
    }
}
