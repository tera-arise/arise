namespace Arise.Net.Packets.Client;

public sealed class C_ARISE_WATCHDOG_REPORT : AriseGamePacket
{
    public override AriseGamePacketCode Code => AriseGamePacketCode.C_ARISE_WATCHDOG_REPORT;

    public required ReadOnlyMemory<byte> Content { get; init; }
}
