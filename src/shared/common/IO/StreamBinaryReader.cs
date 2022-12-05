namespace Arise.IO;

public class StreamBinaryReader
{
    private readonly Stream _stream;

    private readonly Memory<byte> _buffer = GC.AllocateUninitializedArray<byte>(sizeof(ulong));

    public StreamBinaryReader(Stream stream)
    {
        _stream = stream;
    }

    public void Read(Span<byte> buffer)
    {
        _stream.ReadExactly(buffer);
    }

    public ValueTask ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _stream.ReadExactlyAsync(buffer, cancellationToken);
    }

    private unsafe T Read<T>()
        where T : unmanaged
    {
        var span = _buffer[..sizeof(T)].Span;

        Read(span);

        if (!BitConverter.IsLittleEndian)
            span.Reverse();

        return MemoryMarshal.Read<T>(span);
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

    public byte ReadUInt8()
    {
        return Read<byte>();
    }

    public ValueTask<byte> ReadUInt8Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<byte>(cancellationToken);
    }

    public sbyte ReadInt8()
    {
        return Read<sbyte>();
    }

    public ValueTask<sbyte> ReadInt8Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<sbyte>(cancellationToken);
    }

    public ushort ReadUInt16()
    {
        return Read<ushort>();
    }

    public ValueTask<ushort> ReadUInt16Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<ushort>(cancellationToken);
    }

    public short ReadInt16()
    {
        return Read<short>();
    }

    public ValueTask<short> ReadInt16Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<short>(cancellationToken);
    }

    public uint ReadUInt32()
    {
        return Read<uint>();
    }

    public ValueTask<uint> ReadUInt32Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<uint>(cancellationToken);
    }

    public int ReadInt32()
    {
        return Read<int>();
    }

    public ValueTask<int> ReadInt32Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<int>(cancellationToken);
    }

    public ulong ReadUInt64()
    {
        return Read<ulong>();
    }

    public ValueTask<ulong> ReadUInt64Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<ulong>(cancellationToken);
    }

    public long ReadInt64()
    {
        return Read<long>();
    }

    public ValueTask<long> ReadInt64Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<long>(cancellationToken);
    }

    public UInt128 ReadUInt128()
    {
        return Read<UInt128>();
    }

    public ValueTask<UInt128> ReadUInt128Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<UInt128>(cancellationToken);
    }

    public Int128 ReadInt128()
    {
        return Read<Int128>();
    }

    public ValueTask<Int128> ReadInt128Async(CancellationToken cancellationToken = default)
    {
        return ReadAsync<Int128>(cancellationToken);
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

    public Half ReadHalf()
    {
        return Read<Half>();
    }

    public ValueTask<Half> ReadHalfAsync(CancellationToken cancellationToken = default)
    {
        return ReadAsync<Half>(cancellationToken);
    }

    public float ReadSingle()
    {
        return Read<float>();
    }

    public ValueTask<float> ReadSingleAsync(CancellationToken cancellationToken = default)
    {
        return ReadAsync<float>(cancellationToken);
    }

    public double ReadDouble()
    {
        return Read<double>();
    }

    public ValueTask<double> ReadDoubleAsync(CancellationToken cancellationToken = default)
    {
        return ReadAsync<double>(cancellationToken);
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
}
