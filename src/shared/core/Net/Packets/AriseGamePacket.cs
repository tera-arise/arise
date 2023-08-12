namespace Arise.Net.Packets;

public abstract class AriseGamePacket : GamePacket
{
    public override sealed GameConnectionChannel Channel => GameConnectionChannel.Arise;

    public override sealed ushort RawCode => (ushort)Code;

    public abstract AriseGamePacketCode Code { get; }
}
