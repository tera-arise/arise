// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Net.Packets;

public abstract class GamePacket
{
    public abstract GamePacketCode Code { get; }

    private protected GamePacket()
    {
    }

    internal abstract void Serialize(GameStreamAccessor accessor);
}
