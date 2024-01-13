using Arise.Net.Serialization;

namespace Arise.Net.Packets;

public abstract class TeraGamePacket : GamePacket<TeraGamePacketCode>
{
    public override sealed GameConnectionChannel Channel => GameConnectionChannel.Tera;

    private protected TeraGamePacket()
    {
    }

    internal override void Serialize(GameStreamAccessor accessor)
    {
        TeraGamePacketSerializer.Instance.SerializePacket(this, accessor);
    }
}
