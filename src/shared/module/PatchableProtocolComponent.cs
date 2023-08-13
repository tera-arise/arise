namespace Arise.Bridge;

public sealed class PatchableProtocolComponent : ProtocolComponent
{
    [SpecialName]
    protected override void InitializeTeraCodes(Dictionary<TeraGamePacketCode, ushort> codes)
    {
        // Filled in by the server's ModuleProvider.

        codes.Add(default, 42);
    }

    [SpecialName]
    protected override void InitializeAriseCodes(Dictionary<AriseGamePacketCode, ushort> codes)
    {
        // Filled in by the server's ModuleProvider.

        codes.Add(default, 42);
    }
}
