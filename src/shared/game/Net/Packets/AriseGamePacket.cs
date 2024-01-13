using Arise.Net.Serialization;

namespace Arise.Net.Packets;

public abstract class AriseGamePacket : GamePacket<AriseGamePacketCode>
{
    public override sealed GameConnectionChannel Channel => GameConnectionChannel.Arise;

    private protected AriseGamePacket()
    {
    }

    internal override void Serialize(GameStreamAccessor accessor)
    {
        AriseGamePacketSerializer.Instance.SerializePacket(this, accessor);
    }
}
