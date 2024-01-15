using Arise.Bridge;
using Arise.Net.Packets;

namespace Arise.Net;

internal sealed class GameConnectionBuffer
{
    // The packet format is designed to ensure that we can simply slice off the channel and be left with a packet that
    // is fully compatible with the original client. This makes it easier to integrate the protocol.

    // Needed in GameStreamAccessor for ReadOffset/WriteOffset.
    internal const int TeraHeaderSize = sizeof(ushort) * 2;

    private const int AriseHeaderSize = sizeof(ushort) + TeraHeaderSize;

    private const int MaxPayloadSize = ushort.MaxValue - TeraHeaderSize;

    public Memory<byte> Header => _data.AsMemory(0, AriseHeaderSize);

    // Only safe to access with a valid header.
    public Memory<byte> Payload => _data.AsMemory(AriseHeaderSize, Length);

    // Only safe to access with a valid header.
    public Memory<byte> Packet => _data.AsMemory(0, AriseHeaderSize + Length);

    public SlimMemoryStream PayloadStream { get; }

    public GameStreamAccessor PayloadAccessor { get; }

    public GameConnectionChannel Channel
    {
        get => (GameConnectionChannel)BinaryPrimitives.ReadUInt16LittleEndian(_data);
        set => BinaryPrimitives.WriteUInt16LittleEndian(_data, (ushort)value);
    }

    public ushort Length
    {
        get => (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(sizeof(ushort))) - TeraHeaderSize);
        set => BinaryPrimitives.WriteUInt16LittleEndian(_data.AsSpan(sizeof(ushort)), (ushort)(TeraHeaderSize + value));
    }

    public ushort Code
    {
        get => BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(sizeof(ushort) * 2));
        set => BinaryPrimitives.WriteUInt16LittleEndian(_data.AsSpan(sizeof(ushort) * 2), value);
    }

    public bool IsValid => Length <= MaxPayloadSize && Channel switch
    {
        GameConnectionChannel.Tera => Enum.IsDefined((TeraGamePacketCode)Code),
        GameConnectionChannel.Arise => Enum.IsDefined((AriseGamePacketCode)Code),
        _ => false,
    };

    private readonly byte[] _data = GC.AllocateUninitializedArray<byte>(AriseHeaderSize + MaxPayloadSize);

    public GameConnectionBuffer()
    {
        PayloadStream = SlimMemoryStream.CreateEmpty();
        PayloadAccessor = new(PayloadStream);
    }

    public void ResetStream(int? length)
    {
        PayloadStream.SetBuffer(_data.AsMemory(AriseHeaderSize, length ?? MaxPayloadSize));
    }

    public void ConvertToSession(BridgeProtocolComponent protocol)
    {
        Code = Channel switch
        {
            GameConnectionChannel.Tera => protocol.TeraRealToSession[(TeraGamePacketCode)Code],
            GameConnectionChannel.Arise => protocol.AriseRealToSession[(AriseGamePacketCode)Code],
            _ => throw new UnreachableException(),
        };
    }

    public bool TryConvertToReal(BridgeProtocolComponent protocol)
    {
        var (exists, code) = Channel switch
        {
            GameConnectionChannel.Tera => (protocol.TeraSessionToReal.TryGetValue(Code, out var real), (ushort)real),
            GameConnectionChannel.Arise => (protocol.AriseSessionToReal.TryGetValue(Code, out var real), (ushort)real),
            _ => (false, default),
        };

        if (!exists || Length >= MaxPayloadSize)
            return false;

        Code = code;

        return true;
    }
}