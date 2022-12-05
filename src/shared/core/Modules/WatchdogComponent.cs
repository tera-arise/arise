using Arise.IO;

namespace Arise.Modules;

public abstract class WatchdogComponent
{
    public abstract void WriteReport(GameBinaryWriter writer);
}
