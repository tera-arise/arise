namespace Arise.Net.Packets;

public abstract class TeraGamePacket : GamePacket
{
    public override sealed GameConnectionChannel Channel => GameConnectionChannel.Tera;

    public override sealed ushort RawCode => (ushort)Code;

    public abstract TeraGamePacketCode Code { get; }
}
