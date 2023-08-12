namespace Arise.IO;

public class StreamAccessor
{
    public Stream Stream { get; }

    private readonly Memory<byte> _buffer = GC.AllocateUninitializedArray<byte>(sizeof(ulong));

    public StreamAccessor(Stream stream)
    {
        Stream = stream;
    }

    public void Read(Span<byte> buffer)
    {
        Stream.ReadExactly(buffer);
    }

    public ValueTask ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return Stream.ReadExactlyAsync(buffer, cancellationToken);
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        Stream.Write(buffer);
    }

    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return Stream.WriteAsync(buffer, cancellationToken);
    }

    private unsafe T Read<T>()
        where T : unmanaged
    {
        Unsafe.SkipInit(out T value);

        var span = MemoryMarshal.AsBytes(new Span<T>(ref value));

        Read(span);

        if (!BitConverter.IsLittleEndian)
            span.Reverse();

        return value;
    }

    private async ValueTask<T> ReadAsync<T>(CancellationToken cancellationToken)
        where T : unmanaged
    {
        var mem = _buffer[..Unsafe.SizeOf<T>()];

        await ReadAsync(mem, cancellationToken).ConfigureAwait(false);

        if (!BitConverter.IsLittleEndian)
            mem.Span.Reverse();

        return MemoryMarshal.Read<T>(mem.Span);
    }

    private unsafe void Write<T>(T value)
        where T : unmanaged
    {
        var span = MemoryMarshal.AsBytes(new Span<T>(ref value));

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

    public byte ReadUInt8()
    {
        return Read<byte>();
    }

    public ValueTask<byte> ReadUInt8Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<byte>(cancellationToken);
    }

    public void WriteUInt8(byte value)
    {
        Write(value);
    }

    public ValueTask WriteUInt8Async(byte value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public sbyte ReadInt8()
    {
        return Read<sbyte>();
    }

    public ValueTask<sbyte> ReadInt8Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<sbyte>(cancellationToken);
    }

    public void WriteInt8(sbyte value)
    {
        Write(value);
    }

    public ValueTask WriteInt8Async(sbyte value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public ushort ReadUInt16()
    {
        return Read<ushort>();
    }

    public ValueTask<ushort> ReadUInt16Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<ushort>(cancellationToken);
    }

    public void WriteUInt16(ushort value)
    {
        Write(value);
    }

    public ValueTask WriteUInt16Async(ushort value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public short ReadInt16()
    {
        return Read<short>();
    }

    public ValueTask<short> ReadInt16Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<short>(cancellationToken);
    }

    public void WriteInt16(short value)
    {
        Write(value);
    }

    public ValueTask WriteInt16Async(short value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public uint ReadUInt32()
    {
        return Read<uint>();
    }

    public ValueTask<uint> ReadUInt32Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<uint>(cancellationToken);
    }

    public void WriteUInt32(uint value)
    {
        Write(value);
    }

    public ValueTask WriteUInt32Async(uint value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public int ReadInt32()
    {
        return Read<int>();
    }

    public ValueTask<int> ReadInt32Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<int>(cancellationToken);
    }

    public void WriteInt32(int value)
    {
        Write(value);
    }

    public ValueTask WriteInt32Async(int value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public ulong ReadUInt64()
    {
        return Read<ulong>();
    }

    public ValueTask<ulong> ReadUInt64Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<ulong>(cancellationToken);
    }

    public void WriteUInt64(ulong value)
    {
        Write(value);
    }

    public ValueTask WriteUInt64Async(ulong value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public long ReadInt64()
    {
        return Read<long>();
    }

    public ValueTask<long> ReadInt64Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<long>(cancellationToken);
    }

    public void WriteInt64(long value)
    {
        Write(value);
    }

    public ValueTask WriteInt64Async(long value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public UInt128 ReadUInt128()
    {
        return Read<UInt128>();
    }

    public ValueTask<UInt128> ReadUInt128Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<UInt128>(cancellationToken);
    }

    public void WriteUInt128(UInt128 value)
    {
        Write(value);
    }

    public ValueTask WriteUInt128Async(UInt128 value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public Int128 ReadInt128()
    {
        return Read<Int128>();
    }

    public ValueTask<Int128> ReadInt128Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<Int128>(cancellationToken);
    }

    public void WriteInt128(Int128 value)
    {
        Write(value);
    }

    public ValueTask WriteInt128Async(Int128 value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public T ReadEnum<T>()
        where T : unmanaged, Enum
    {
        return Read<T>();
    }

    public ValueTask<T> ReadEnumAsync<T>(CancellationToken cancellationToken = default)
        where T : unmanaged, Enum
    {
        return ReadAsync<T>(cancellationToken);
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

    public Half ReadHalf()
    {
        return Read<Half>();
    }

    public ValueTask<Half> ReadHalfAsync(CancellationToken cancellationToken = default)
    {
        return ReadAsync<Half>(cancellationToken);
    }

    public void WriteHalf(Half value)
    {
        Write(value);
    }

    public ValueTask WriteHalfAsync(Half value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public float ReadSingle()
    {
        return Read<float>();
    }

    public ValueTask<float> ReadSingleAsync(CancellationToken cancellationToken = default)
    {
        return ReadAsync<float>(cancellationToken);
    }

    public void WriteSingle(float value)
    {
        Write(value);
    }

    public ValueTask WriteSingleAsync(float value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public double ReadDouble()
    {
        return Read<double>();
    }

    public ValueTask<double> ReadDoubleAsync(CancellationToken cancellationToken = default)
    {
        return ReadAsync<double>(cancellationToken);
    }

    public void WriteDouble(double value)
    {
        Write(value);
    }

    public ValueTask WriteDoubleAsync(double value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(value, cancellationToken);
    }

    public bool ReadBoolean()
    {
        // https://github.com/dotnet/roslyn/blob/main/docs/compilers/Boolean%20Representation.md
        return ReadUInt8() != 0;
    }

    public async ValueTask<bool> ReadBooleanAsync(CancellationToken cancellationToken = default)
    {
        // https://github.com/dotnet/roslyn/blob/main/docs/compilers/Boolean%20Representation.md
        return (await ReadUInt8Async(cancellationToken).ConfigureAwait(false)) != 0;
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
