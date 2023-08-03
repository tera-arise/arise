namespace Arise.Module;

public sealed class GameProtocolComponent : ProtocolComponent
{
    public override IReadOnlyDictionary<Type, ushort> PacketCodes { get; }

    public GameProtocolComponent()
    {
        PacketCodes = GetPacketCodes();
    }

    [Obfuscation]
    private static IReadOnlyDictionary<Type, ushort> GetPacketCodes()
    {
        var codes = new Dictionary<Type, ushort>();

        // TODO: Fill in packet codes.

        return codes;
    }
}
