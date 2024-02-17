namespace Arise.Net.Packets.Server;

public sealed class S_CHECK_VERSION : TeraGamePacket
{
    public override GamePacketCode Code => GamePacketCode.S_CHECK_VERSION;

    public required bool IsCompatible { get; init; }
}
