using Arise.Bridge.Protection;

namespace Arise.Bridge;

public sealed class PatchableBridgeProtectionComponent : BridgeProtectionComponent
{
    private readonly IEnumerable<GameProtectionTask> _tasks = new GameProtectionTask[]
    {
        new CodeChecksumTask(),
        new DebuggerDetectionTask(),
    };

    [Obfuscation]
    public override void Start()
    {
        // This method body is erased by the server's BridgeModuleGenerator for module instances that run on the server
        // and for module instances running on the client in development scenarios.

        foreach (var task in _tasks)
            task.Start();
    }

    [Obfuscation]
    public override void Stop()
    {
        // This method body is erased by the server's BridgeModuleGenerator for module instances that run on the server
        // and for module instances running on the client in development scenarios.

        foreach (var task in _tasks.Reverse())
            task.Stop();
    }
}
