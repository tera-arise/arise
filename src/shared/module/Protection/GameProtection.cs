using static Arise.Module.WindowsPInvoke;

namespace Arise.Module.Protection;

internal static class GameProtection
{
    [SpecialName]
    public static void Initialize()
    {
        // This method body is erased by the server's BridgeModuleProvider for module instances that run on the server
        // and for module instances running on the client in development scenarios.

        var culture = CultureInfo.InvariantCulture;

        if (DateTime.UtcNow - DateTime.Parse(GetIssueTime(), culture) > TimeSpan.Parse(GetValidDuration(), culture))
            Terminate();

        new CodeChecksumTask().Start();
        new DebuggerDetectionTask().Start();
    }

    [SpecialName]
    private static string GetIssueTime()
    {
        // Filled in by the server's ModuleProvider.
        return "xyz";
    }

    [SpecialName]
    private static string GetValidDuration()
    {
        // Filled in by the server's ModuleProvider.
        return "xyz";
    }

    [SuppressMessage("", "CA5394")]
    public static void Terminate()
    {
        _ = Task.Run(static async () =>
        {
            var (lower, upper) = GetExitDelayRange();

            await Task.Delay(Random.Shared.Next(lower, upper)).ConfigureAwait(false);

            _ = NtTerminateProcess(NtCurrentProcess, GetExitStatus());
        });
    }

    [SpecialName]
    private static (int Lower, int Upper) GetExitDelayRange()
    {
        // Filled in by the server's BridgeModuleProvider.
        return (42, 42);
    }

    [SpecialName]
    private static int GetExitStatus()
    {
        // Filled in by the server's BridgeModuleProvider.
        return 42;
    }
}
