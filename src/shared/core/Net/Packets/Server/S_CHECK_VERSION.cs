namespace Arise.Net.Packets.Server;

public sealed class S_CHECK_VERSION : TeraGamePacket
{
    public override TeraGamePacketCode Code => TeraGamePacketCode.S_CHECK_VERSION;

    public required bool IsCompatible { get; init; }
}
