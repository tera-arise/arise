namespace Arise.Bridge;

public abstract class BridgeModule
{
    public abstract BridgeDataComponent Data { get; }

    public abstract BridgeProtocolComponent Protocol { get; }

    public abstract BridgeWatchdogComponent Watchdog { get; }
}
