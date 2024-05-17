// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Net.Serialization;

namespace Arise.Net.Packets;

public abstract class AriseGamePacket : GamePacket
{
    private protected AriseGamePacket()
    {
    }

    internal override void Serialize(GameStreamAccessor accessor)
    {
        AriseGamePacketSerializer.Instance.SerializePacket(this, accessor);
    }
}
