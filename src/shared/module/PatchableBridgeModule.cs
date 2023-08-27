namespace Arise.Bridge;

public sealed class PatchableBridgeModule : BridgeModule
{
    public override PatchableBridgeDataComponent Data { get; } = new();

    public override PatchableBridgeProtectionComponent Protection { get; } = new();

    public override PatchableBridgeProtocolComponent Protocol { get; } = new();

    public override PatchableBridgeWatchdogComponent Watchdog { get; } = new();
}
