using static Arise.Bridge.WindowsPInvoke;

namespace Arise.Bridge.Protection;

internal static class GameProtection
{
    private static int _initialized;

    [Obfuscation]
    public static void Initialize()
    {
        // This method body is erased by the server's BridgeModuleGenerator for module instances that run on the server
        // and for module instances running on the client in development scenarios.

        if (Interlocked.Exchange(ref _initialized, 1) == 1)
            return;

        var culture = CultureInfo.InvariantCulture;

        if (DateTime.UtcNow - DateTime.Parse(GetIssueTime(), culture) > TimeSpan.Parse(GetValidDuration(), culture))
            Terminate();
    }

    [Obfuscation]
    private static string GetIssueTime()
    {
        // Filled in by the server's BridgeModuleGenerator.
        return "xyz";
    }

    [Obfuscation]
    private static string GetValidDuration()
    {
        // Filled in by the server's BridgeModuleGenerator.
        return "xyz";
    }

    public static void Terminate()
    {
        _ = Task.Run(static async () =>
        {
            await Task.Delay(GetExitDelay()).ConfigureAwait(false);

            _ = NtTerminateProcess(NtCurrentProcess, GetExitStatus());
        });
    }

    [Obfuscation]
    private static int GetExitDelay()
    {
        // Filled in by the server's BridgeModuleGenerator.
        return 42;
    }

    [Obfuscation]
    private static int GetExitStatus()
    {
        // Filled in by the server's BridgeModuleGenerator.
        return 42;
    }
}
