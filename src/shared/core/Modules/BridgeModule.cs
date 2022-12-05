namespace Arise.Modules;

public abstract class BridgeModule
{
    public abstract DataComponent Data { get; }

    public abstract ProtocolComponent Protocol { get; }

    public abstract WatchdogComponent Watchdog { get; }
}
