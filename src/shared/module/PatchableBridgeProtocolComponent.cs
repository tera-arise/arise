namespace Arise.Bridge;

public sealed class PatchableBridgeProtocolComponent : BridgeProtocolComponent
{
    [Obfuscation]
    protected override void InitializeTeraCodes(Dictionary<TeraGamePacketCode, ushort> codes)
    {
        // Filled in by the server's BridgeModuleProvider.

        codes.Add(default, 42);
    }

    [Obfuscation]
    protected override void InitializeAriseCodes(Dictionary<AriseGamePacketCode, ushort> codes)
    {
        // Filled in by the server's BridgeModuleProvider.

        codes.Add(default, 42);
    }
}
