// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Bridge.Protection;

namespace Arise.Bridge;

public sealed class PatchableBridgeProtectionComponent : BridgeProtectionComponent
{
    private readonly Queue<GameProtectionTask> _tasks = new();

    public override void Start()
    {
        _tasks.Enqueue(new CodeChecksumTask());
        _tasks.Enqueue(new DebuggerDetectionTask());

        foreach (var task in _tasks)
            task.Start();
    }

    public override void Stop()
    {
        foreach (var task in _tasks.Reverse())
            task.Stop();
    }
}
