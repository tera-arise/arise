namespace Arise.Modules;

public abstract class ProtocolComponent
{
    public abstract ImmutableDictionary<ushort, ushort> Codes { get; }
}
