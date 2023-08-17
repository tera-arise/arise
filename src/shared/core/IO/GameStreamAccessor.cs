using Arise.Entities;

namespace Arise.IO;

public class GameStreamAccessor : StreamAccessor
{
    private static readonly UTF8Encoding _utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private static readonly UnicodeEncoding _utf16 =
        new(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true);

    public GameStreamAccessor(Stream stream)
        : base(stream)
    {
    }

    public ushort ReadCompactUInt16()
    {
        return ReadUInt8() switch
        {
            0xff => ReadUInt16(),
            var value => value,
        };
    }

    public void WriteCompactUInt16(ushort value)
    {
        switch (value)
        {
            case < 0xff:
                WriteUInt8((byte)value);
                break;
            default:
                WriteUInt8(0xff);
                WriteUInt16(value);
                break;
        }
    }

    public short ReadCompactInt16()
    {
        return ReadInt8() switch
        {
            0x7f => ReadInt16(),
            var value => value,
        };
    }

    public void WriteCompactInt16(short value)
    {
        switch (value)
        {
            case >= -0x80 and < 0x7f:
                WriteInt8((sbyte)value);
                break;
            default:
                WriteInt8(0x7f);
                WriteInt16(value);
                break;
        }
    }

    public uint ReadCompactUInt32()
    {
        return ReadUInt8() switch
        {
            0xfd => ReadUInt16(),
            0xfe => (uint)(ReadUInt16() | ReadUInt8() << 16),
            0xff => ReadUInt32(),
            var value => value,
        };
    }

    public void WriteCompactUInt32(uint value)
    {
        switch (value)
        {
            case < 0xfd:
                WriteUInt8((byte)value);
                break;
            case <= 0xffff:
                WriteUInt8(0xfd);
                WriteUInt16((ushort)value);
                break;
            case <= 0xffffff:
                WriteUInt8(0xfe);
                WriteUInt16((ushort)value);
                WriteUInt8((byte)(value >> 16));
                break;
            default:
                WriteUInt8(0xff);
                WriteUInt32(value);
                break;
        }
    }

    public int ReadCompactInt32()
    {
        return ReadInt8() switch
        {
            0x7b => ~ReadUInt16(),
            0x7c => ReadUInt16(),
            0x7d => ~(ReadUInt16() | ReadUInt8() << 16),
            0x7e => ReadUInt16() | ReadUInt8() << 16,
            0x7f => ReadInt32(),
            var value => value,
        };
    }

    public void WriteCompactInt32(int value)
    {
        var sign = value < 0;
        var compl = ~value;

        switch (value)
        {
            case >= -0x80 and < 0x7b:
                WriteInt8((sbyte)value);
                break;
            case >= -0x10000 and <= 0xffff:
                WriteInt8((sbyte)(sign ? 0x7b : 0x7c));
                WriteUInt16((ushort)(sign ? compl : value));
                break;
            case >= -0x1000000 and <= 0xffffff:
                WriteInt8((sbyte)(sign ? 0x7d : 0x7e));
                WriteUInt16((ushort)(sign ? compl : value));
                WriteUInt8((byte)((sign ? compl : value) >>> 16));
                break;
            default:
                WriteInt8(0x7f);
                WriteInt32(value);
                break;
        }
    }

    public ulong ReadCompactUInt64()
    {
        return ReadUInt8() switch
        {
            0xf9 => ReadUInt16(),
            0xfa => (uint)(ReadUInt16() | ReadUInt8() << 16),
            0xfb => ReadUInt32(),
            0xfc => ReadUInt32() | (ulong)ReadUInt8() << 32,
            0xfd => ReadUInt32() | (ulong)ReadUInt16() << 32,
            0xfe => ReadUInt32() | (ulong)ReadUInt16() << 32 | (ulong)ReadUInt8() << 48,
            0xff => ReadUInt64(),
            var value => value,
        };
    }

    public void WriteCompactUInt64(ulong value)
    {
        switch (value)
        {
            case < 0xf9:
                WriteUInt8((byte)value);
                break;
            case <= 0xffff:
                WriteUInt8(0xf9);
                WriteUInt16((ushort)value);
                break;
            case <= 0xffffff:
                WriteUInt8(0xfa);
                WriteUInt16((ushort)value);
                WriteUInt8((byte)(value >> 16));
                break;
            case <= 0xffffffff:
                WriteUInt8(0xfb);
                WriteUInt32((uint)value);
                break;
            case <= 0xffffffffff:
                WriteUInt8(0xfc);
                WriteUInt32((uint)value);
                WriteUInt8((byte)(value >> 32));
                break;
            case <= 0xffffffffffff:
                WriteUInt8(0xfd);
                WriteUInt32((uint)value);
                WriteUInt16((ushort)(value >> 32));
                break;
            case <= 0xffffffffffffff:
                WriteUInt8(0xfe);
                WriteUInt32((uint)value);
                WriteUInt16((ushort)(value >> 32));
                WriteUInt8((byte)(value >> 48));
                break;
            default:
                WriteUInt8(0xff);
                WriteUInt64(value);
                break;
        }
    }

    public long ReadCompactInt64()
    {
        return ReadInt8() switch
        {
            0x73 => ~ReadUInt16(),
            0x74 => ReadUInt16(),
            0x75 => ~(ReadUInt16() | ReadUInt8() << 16),
            0x76 => ReadUInt16() | ReadUInt8() << 16,
            0x77 => ~ReadUInt32(),
            0x78 => ReadUInt32(),
            0x79 => ~(ReadUInt32() | (long)ReadUInt8() << 32),
            0x7a => ReadUInt32() | (long)ReadUInt8() << 32,
            0x7b => ~(ReadUInt32() | (long)ReadUInt16() << 32),
            0x7c => ReadUInt32() | (long)ReadUInt16() << 32,
            0x7d => ~(ReadUInt32() | (long)ReadUInt16() << 32 | (long)ReadUInt8() << 48),
            0x7e => ReadUInt32() | (long)ReadUInt16() << 32 | (long)ReadUInt8() << 48,
            0x7f => ReadInt64(),
            var value => value,
        };
    }

    public void WriteCompactInt64(long value)
    {
        var sign = value < 0;
        var compl = ~value;

        switch (value)
        {
            case >= -0x80 and < 0x73:
                WriteInt8((sbyte)value);
                break;
            case >= -0x10000 and <= 0xffff:
                WriteInt8((sbyte)(sign ? 0x73 : 0x74));
                WriteUInt16((ushort)(sign ? compl : value));
                break;
            case >= -0x1000000 and <= 0xffffff:
                WriteInt8((sbyte)(sign ? 0x75 : 0x76));
                WriteUInt16((ushort)(sign ? compl : value));
                WriteUInt8((byte)((sign ? compl : value) >> 16));
                break;
            case >= -0x100000000 and <= 0xffffffff:
                WriteInt8((sbyte)(sign ? 0x77 : 0x78));
                WriteUInt32((uint)(sign ? compl : value));
                break;
            case >= -0x10000000000 and <= 0xffffffff:
                WriteInt8((sbyte)(sign ? 0x79 : 0x7a));
                WriteUInt32((uint)(sign ? compl : value));
                WriteUInt8((byte)((sign ? compl : value) >>> 32));
                break;
            case >= -0x1000000000000 and <= 0xffffffffffff:
                WriteInt8((sbyte)(sign ? 0x7b : 0x7c));
                WriteUInt32((uint)(sign ? compl : value));
                WriteUInt16((ushort)((sign ? compl : value) >>> 32));
                break;
            case >= -0x100000000000000 and <= 0xffffffffffffff:
                WriteInt8((sbyte)(sign ? 0x7d : 0x7e));
                WriteUInt32((uint)(sign ? compl : value));
                WriteUInt16((ushort)((sign ? compl : value) >>> 32));
                WriteUInt8((byte)((sign ? compl : value) >> 48));
                break;
            default:
                WriteInt8(0x7f);
                WriteInt64(value);
                break;
        }
    }

    // TODO: int128

    public T ReadCompactEnum<T>()
        where T : unmanaged, Enum
    {
        if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) || TypeFacts<T>.IsFlags)
            return ReadEnum<T>();
        else if (typeof(T) == typeof(ushort))
            return Unsafe.BitCast<ushort, T>(ReadCompactUInt16());
        else if (typeof(T) == typeof(short))
            return Unsafe.BitCast<short, T>(ReadCompactInt16());
        else if (typeof(T) == typeof(uint))
            return Unsafe.BitCast<uint, T>(ReadCompactUInt32());
        else if (typeof(T) == typeof(int))
            return Unsafe.BitCast<int, T>(ReadCompactInt32());
        else if (typeof(T) == typeof(ulong))
            return Unsafe.BitCast<ulong, T>(ReadCompactUInt64());
        else if (typeof(T) == typeof(long))
            return Unsafe.BitCast<long, T>(ReadCompactInt64());

        throw new UnreachableException();
    }

    public void WriteCompactEnum<T>(T value)
        where T : unmanaged, Enum
    {
        if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) || TypeFacts<T>.IsFlags)
            WriteEnum(value);
        else if (typeof(T) == typeof(ushort))
            WriteCompactUInt16(Unsafe.BitCast<T, ushort>(value));
        else if (typeof(T) == typeof(short))
            WriteCompactInt16(Unsafe.BitCast<T, short>(value));
        else if (typeof(T) == typeof(uint))
            WriteCompactUInt32(Unsafe.BitCast<T, uint>(value));
        else if (typeof(T) == typeof(int))
            WriteCompactInt32(Unsafe.BitCast<T, int>(value));
        else if (typeof(T) == typeof(ulong))
            WriteCompactUInt64(Unsafe.BitCast<T, ulong>(value));
        else if (typeof(T) == typeof(long))
            WriteCompactInt64(Unsafe.BitCast<T, long>(value));

        throw new UnreachableException();
    }

    public string ReadString()
    {
        var length = 0;
        var buffer = ArrayPool<char>.Shared.Rent(1024);

        string str;

        try
        {
            char ch;

            // This loop is kind of terrible, but there is not much we can do about it.
            while ((ch = (char)ReadUInt16()) != char.MinValue)
            {
                if (length > buffer.Length)
                {
                    ArrayPool<char>.Shared.Return(buffer);

                    buffer = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                }

                buffer[length++] = ch;
            }

            if (length == 0)
                return string.Empty;

            str = new(buffer, 0, length);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }

        try
        {
            _ = _utf16.GetByteCount(str);
        }
        catch (EncoderFallbackException)
        {
            throw new InvalidDataException();
        }

        return str;
    }

    public void WriteString(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var buffer = ArrayPool<byte>.Shared.Rent(_utf16.GetMaxByteCount(value.Length));

            try
            {
                Write(buffer.AsSpan(0, _utf16.GetBytes(value, buffer)));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        WriteUInt16(char.MinValue);
    }

    public string ReadCompactString()
    {
        var length = ReadCompactUInt16();

        if (length == 0)
            return string.Empty;

        var buffer = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            var span = buffer.AsSpan(0, length);

            Read(span);

            return _utf8.GetString(span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void WriteCompactString(string? value)
    {
        var length = (ushort)_utf8.GetByteCount(value.AsSpan());

        WriteCompactUInt16(length);

        if (length == 0)
            return;

        var buffer = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            Write(buffer.AsSpan(0, _utf8.GetBytes(value, buffer)));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
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

    public EntityId ReadEntityId()
    {
        return new(ReadInt32(), ReadEnum<EntityType>());
    }

    public void WriteEntityId(EntityId value)
    {
        WriteInt32(value.Id);
        WriteEnum(value.Type);
    }

    public EntityId ReadCompactEntityId()
    {
        // TODO: Consider compact encoding for the type once EntityType is fully understood.
        return new(ReadCompactInt32(), ReadEnum<EntityType>());
    }

    public void WriteCompactEntityId(EntityId value)
    {
        WriteCompactInt32(value.Id);
        WriteEnum(value.Type);
    }
}
