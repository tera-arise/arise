namespace Arise.Bridge;

public sealed class PatchableBridgeModule : BridgeModule
{
    public override PatchableDataComponent Data { get; } = new PatchableDataComponent();

    public override PatchableProtocolComponent Protocol { get; } = new PatchableProtocolComponent();

    public override PatchableWatchdogComponent Watchdog { get; } = new PatchableWatchdogComponent();
}
