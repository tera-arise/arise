namespace Arise.Modules;

public abstract class DataComponent
{
    public abstract ReadOnlyMemory<byte> Key { get; }

    public abstract ReadOnlyMemory<byte> IV { get; }
}
