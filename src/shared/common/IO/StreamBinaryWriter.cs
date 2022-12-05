namespace Arise.IO;

public class StreamBinaryWriter
{
    private readonly Stream _stream;

    private readonly Memory<byte> _buffer = GC.AllocateUninitializedArray<byte>(sizeof(ulong));

    public StreamBinaryWriter(Stream stream)
    {
        _stream = stream;
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        _stream.Write(buffer);
    }

    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _stream.WriteAsync(buffer, cancellationToken);
    }

    private unsafe void Write<T>(T value)
        where T : unmanaged
    {
        var span = _buffer.Span[..sizeof(T)];

        MemoryMarshal.Write(span, ref value);

        if (!BitConverter.IsLittleEndian)
            span.Reverse();

        Write(span);
    }

    private ValueTask WriteAsync<T>(T value, CancellationToken cancellationToken)
        where T : unmanaged
    {
        var mem = _buffer[..Unsafe.SizeOf<T>()];

        MemoryMarshal.Write(mem.Span, ref value);

        if (!BitConverter.IsLittleEndian)
            mem.Span.Reverse();

        return WriteAsync(mem, cancellationToken);
    }

    public void WriteUInt8(byte value)
    {
        Write(value);
    }

    public ValueTask WriteUInt8Async(byte value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteInt8(sbyte value)
    {
        Write(value);
    }

    public ValueTask WriteInt8Async(sbyte value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteUInt16(ushort value)
    {
        Write(value);
    }

    public ValueTask WriteUInt16Async(ushort value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteInt16(short value)
    {
        Write(value);
    }

    public ValueTask WriteInt16Async(short value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteUInt32(uint value)
    {
        Write(value);
    }

    public ValueTask WriteUInt32Async(uint value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteInt32(int value)
    {
        Write(value);
    }

    public ValueTask WriteInt32Async(int value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteUInt64(ulong value)
    {
        Write(value);
    }

    public ValueTask WriteUInt64Async(ulong value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteInt64(long value)
    {
        Write(value);
    }

    public ValueTask WriteInt64Async(long value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteUInt128(UInt128 value)
    {
        Write(value);
    }

    public ValueTask WriteUInt128Async(UInt128 value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteInt128(Int128 value)
    {
        Write(value);
    }

    public ValueTask WriteInt128Async(Int128 value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteEnum<T>(T value)
        where T : unmanaged, Enum
    {
        Write(value);
    }

    public ValueTask WriteEnumAsync<T>(T value, CancellationToken cancellationToken = default)
        where T : unmanaged, Enum
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteHalf(Half value)
    {
        Write(value);
    }

    public ValueTask WriteHalfAsync(Half value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteSingle(float value)
    {
        Write(value);
    }

    public ValueTask WriteSingleAsync(float value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteDouble(double value)
    {
        Write(value);
    }

    public ValueTask WriteDoubleAsync(double value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public void WriteBoolean(bool value)
    {
        Write(value);
    }

    public ValueTask WriteBooleanAsync(bool value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }
}
