namespace Arise.IO;

public class StreamAccessor
{
    public Stream Stream { get; }

    public long Length
    {
        get => Stream.Length;
        set => Stream.SetLength(value);
    }

    public long Position
    {
        get => Stream.Position;
        set => Stream.Position = value;
    }

    public StreamAccessor(Stream stream)
    {
        Stream = stream;
    }

    public void Read(scoped Span<byte> buffer)
    {
        Stream.ReadExactly(buffer);
    }

    public void Write(scoped ReadOnlySpan<byte> buffer)
    {
        Stream.Write(buffer);
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

    private unsafe void Write<T>(T value)
        where T : unmanaged
    {
        var span = MemoryMarshal.AsBytes(new Span<T>(ref value));

        if (!BitConverter.IsLittleEndian)
            span.Reverse();

        Write(span);
    }

    public byte ReadByte()
    {
        return Read<byte>();
    }

    public void WriteByte(byte value)
    {
        Write(value);
    }

    public sbyte ReadSByte()
    {
        return Read<sbyte>();
    }

    public void WriteSByte(sbyte value)
    {
        Write(value);
    }

    public ushort ReadUInt16()
    {
        return Read<ushort>();
    }

    public void WriteUInt16(ushort value)
    {
        Write(value);
    }

    public short ReadInt16()
    {
        return Read<short>();
    }

    public void WriteInt16(short value)
    {
        Write(value);
    }

    public uint ReadUInt32()
    {
        return Read<uint>();
    }

    public void WriteUInt32(uint value)
    {
        Write(value);
    }

    public int ReadInt32()
    {
        return Read<int>();
    }

    public void WriteInt32(int value)
    {
        Write(value);
    }

    public ulong ReadUInt64()
    {
        return Read<ulong>();
    }

    public void WriteUInt64(ulong value)
    {
        Write(value);
    }

    public long ReadInt64()
    {
        return Read<long>();
    }

    public void WriteInt64(long value)
    {
        Write(value);
    }

    public T ReadEnum<T>()
        where T : unmanaged, Enum
    {
        return Read<T>();
    }

    public void WriteEnum<T>(T value)
        where T : unmanaged, Enum
    {
        Write(value);
    }

    public float ReadSingle()
    {
        return Read<float>();
    }

    public void WriteSingle(float value)
    {
        Write(value);
    }

    public double ReadDouble()
    {
        return Read<double>();
    }

    public void WriteDouble(double value)
    {
        Write(value);
    }

    public Vector3 ReadVector3()
    {
        return new(ReadSingle(), ReadSingle(), ReadSingle());
    }

    public void WriteVector3(Vector3 value)
    {
        WriteSingle(value.X);
        WriteSingle(value.Y);
        WriteSingle(value.Z);
    }

    public bool ReadBoolean()
    {
        // https://github.com/dotnet/roslyn/blob/main/docs/compilers/Boolean%20Representation.md
        return ReadByte() != 0;
    }

    public void WriteBoolean(bool value)
    {
        Write(value);
    }

    public char ReadChar()
    {
        return (char)ReadUInt16();
    }

    public void WriteChar(char value)
    {
        WriteUInt16(value);
    }
}
