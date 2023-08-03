namespace Arise.Modules;

public abstract class ProtocolComponent
{
    public abstract IReadOnlyDictionary<Type, ushort> PacketCodes { get; }
}
