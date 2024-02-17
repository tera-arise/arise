namespace Arise.Net.Packets;

public abstract class GamePacket
{
    public abstract GamePacketCode Code { get; }

    private protected GamePacket()
    {
    }

    internal abstract void Serialize(GameStreamAccessor accessor);
}
