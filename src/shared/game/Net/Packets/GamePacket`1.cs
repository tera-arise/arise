namespace Arise.Net.Packets;

public abstract class GamePacket<T> : GamePacket
    where T : unmanaged, Enum
{
    public override ushort RawCode => Unsafe.BitCast<T, ushort>(Code);

    public abstract T Code { get; }

    private protected GamePacket()
    {
    }
}
