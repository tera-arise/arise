namespace Arise.Modules;

public abstract class ProtocolComponent
{
    public abstract int OrderingSeed { get; }

    public abstract IReadOnlyDictionary<Type, int> PacketCodes { get; }
}
