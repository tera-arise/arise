namespace Arise.Modules;

public abstract class WatchdogComponent
{
    public abstract void WriteReport(GameStreamAccessor accessor);
}
