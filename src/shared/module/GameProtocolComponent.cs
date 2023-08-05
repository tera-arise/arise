namespace Arise.Module;

public sealed class GameProtocolComponent : ProtocolComponent
{
    public override ImmutableDictionary<ushort, ushort> Codes { get; }

    public GameProtocolComponent()
    {
        var codes = new Dictionary<ushort, ushort>();

        InitializeCodes(codes);

        Codes = codes.ToImmutableDictionary();
    }

    [SpecialName]
    private static void InitializeCodes(Dictionary<ushort, ushort> codes)
    {
        // Filled in by the server's ModuleProvider.

        codes.Add(42, 42);
    }
}
