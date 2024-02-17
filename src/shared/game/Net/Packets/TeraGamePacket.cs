using Arise.Net.Serialization;

namespace Arise.Net.Packets;

public abstract class TeraGamePacket : GamePacket
{
    private protected TeraGamePacket()
    {
    }

    internal override void Serialize(GameStreamAccessor accessor)
    {
        TeraGamePacketSerializer.Instance.SerializePacket(this, accessor);
    }
}
