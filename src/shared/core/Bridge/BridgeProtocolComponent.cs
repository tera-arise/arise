using Arise.Net.Packets;

namespace Arise.Bridge;

public abstract class BridgeProtocolComponent
{
    public FrozenDictionary<TeraGamePacketCode, ushort> TeraRealToSession { get; }

    public FrozenDictionary<ushort, TeraGamePacketCode> TeraSessionToReal { get; }

    public FrozenDictionary<AriseGamePacketCode, ushort> AriseRealToSession { get; }

    public FrozenDictionary<ushort, AriseGamePacketCode> AriseSessionToReal { get; }

    [SuppressMessage("", "CA2214")]
    protected BridgeProtocolComponent()
    {
        var teraCodes = new Dictionary<TeraGamePacketCode, ushort>();
        var ariseCodes = new Dictionary<AriseGamePacketCode, ushort>();

        InitializeTeraCodes(teraCodes);
        InitializeAriseCodes(ariseCodes);

        TeraRealToSession = teraCodes.ToFrozenDictionary();
        TeraSessionToReal = teraCodes.ToFrozenDictionary(static kvp => kvp.Value, static kvp => kvp.Key);
        AriseRealToSession = ariseCodes.ToFrozenDictionary();
        AriseSessionToReal = ariseCodes.ToFrozenDictionary(static kvp => kvp.Value, static kvp => kvp.Key);
    }

    protected abstract void InitializeTeraCodes(Dictionary<TeraGamePacketCode, ushort> codes);

    protected abstract void InitializeAriseCodes(Dictionary<AriseGamePacketCode, ushort> codes);
}
