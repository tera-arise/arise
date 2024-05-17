// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Bridge;
using Arise.Net.Packets;

namespace Arise.Net;

internal sealed class GameConnectionBuffer
{
    // Needed in GameStreamAccessor for ReadOffset/WriteOffset.
    internal const int HeaderSize = sizeof(ushort) * 2;

    private const int MaxPayloadSize = ushort.MaxValue - HeaderSize;

    public Memory<byte> Header => _data.AsMemory(0, HeaderSize);

    // Only safe to access with a valid header.
    public Memory<byte> Payload => _data.AsMemory(HeaderSize, Length);

    // Only safe to access with a valid header.
    public Memory<byte> Packet => _data.AsMemory(0, HeaderSize + Length);

    public SlimMemoryStream PayloadStream { get; }

    public GameStreamAccessor PayloadAccessor { get; }

    public ushort Length
    {
        get => (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(_data) - HeaderSize);
        set => BinaryPrimitives.WriteUInt16LittleEndian(_data, (ushort)(HeaderSize + value));
    }

    public ushort Code
    {
        get => BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(sizeof(ushort)));
        set => BinaryPrimitives.WriteUInt16LittleEndian(_data.AsSpan(sizeof(ushort)), value);
    }

    public bool IsValid => Length <= MaxPayloadSize && Enum.IsDefined((GamePacketCode)Code);

    private readonly byte[] _data = GC.AllocateUninitializedArray<byte>(HeaderSize + MaxPayloadSize);

    public GameConnectionBuffer()
    {
        PayloadStream = SlimMemoryStream.CreateEmpty();
        PayloadAccessor = new(PayloadStream);
    }

    public void ResetStream(int? length)
    {
        PayloadStream.SetBuffer(_data.AsMemory(HeaderSize, length ?? MaxPayloadSize));
    }

    public void ConvertToSession(BridgeProtocolComponent protocol)
    {
        Code = protocol.RealToSession[(GamePacketCode)Code];
    }

    public bool TryConvertToReal(BridgeProtocolComponent protocol)
    {
        if (!protocol.SessionToReal.TryGetValue(Code, out var real) || Length >= MaxPayloadSize)
            return false;

        Code = (ushort)real;

        return true;
    }
}
