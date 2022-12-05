namespace Arise.Module;

internal sealed class GameProtocolComponent : ProtocolComponent
{
    public override int OrderingSeed { get; }

    public override IReadOnlyDictionary<Type, int> PacketCodes { get; }

    public GameProtocolComponent()
    {
        OrderingSeed = GetOrderingSeed();
        PacketCodes = GetPacketCodes();
    }

    [Obfuscation]
    private static int GetOrderingSeed()
    {
        return 42;
    }

    [Obfuscation]
    private static IReadOnlyDictionary<Type, int> GetPacketCodes()
    {
        // TODO
        return new Dictionary<Type, int>();
    }
}
