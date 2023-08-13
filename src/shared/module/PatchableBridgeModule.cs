namespace Arise.Bridge;

public sealed class PatchableBridgeModule : BridgeModule
{
    public override PatchableBridgeDataComponent Data { get; } = new PatchableBridgeDataComponent();

    public override PatchableBridgeProtocolComponent Protocol { get; } = new PatchableBridgeProtocolComponent();

    public override PatchableBridgeWatchdogComponent Watchdog { get; } = new PatchableBridgeWatchdogComponent();
}
