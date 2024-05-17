// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Net.Packets.Server;

public sealed class S_ARISE_WATCHDOG_REPORT : AriseGamePacket
{
    public override GamePacketCode Code => GamePacketCode.S_ARISE_WATCHDOG_REPORT;
}
