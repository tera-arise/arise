namespace Arise.Net.Packets;

public abstract class GamePacket
{
    public abstract GameConnectionChannel Channel { get; }

    public abstract ushort RawCode { get; }
}
