// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Net.Packets.Client;

public sealed class C_ARISE_WATCHDOG_REPORT : AriseGamePacket
{
    public override GamePacketCode Code => GamePacketCode.C_ARISE_WATCHDOG_REPORT;

    public required ReadOnlyMemory<byte> Content { get; init; }
}
