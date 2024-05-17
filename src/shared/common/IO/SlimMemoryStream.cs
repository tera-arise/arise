// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.IO;

public sealed class SlimMemoryStream : Stream
{
    public override bool CanRead => true;

    public override bool CanWrite => _writable;

    public override bool CanSeek => true;

    public override long Length => _buffer.Length;

    public override long Position
    {
        get => _position;
        set => _position = (int)value;
    }

    private Memory<byte> _buffer;

    private bool _writable;

    private int _position;

    private SlimMemoryStream()
    {
    }

    public static SlimMemoryStream CreateEmpty()
    {
        return new();
    }

    public static SlimMemoryStream CreateReadOnly(ReadOnlyMemory<byte> buffer)
    {
        var stream = new SlimMemoryStream();

        stream.SetReadOnlyBuffer(buffer);

        return stream;
    }

    public static SlimMemoryStream Create(Memory<byte> buffer)
    {
        var stream = new SlimMemoryStream();

        stream.SetBuffer(buffer);

        return stream;
    }

    public void SetReadOnlyBuffer(ReadOnlyMemory<byte> buffer)
    {
        _buffer = MemoryMarshal.AsMemory(buffer);
        _writable = false;
        _position = 0;
    }

    public void SetBuffer(Memory<byte> buffer)
    {
        _buffer = buffer;
        _writable = true;
        _position = 0;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var pos = origin switch
        {
            SeekOrigin.Begin => (int)offset,
            SeekOrigin.Current => _position + (int)offset,
            SeekOrigin.End => _buffer.Length + (int)offset,
            _ => throw new UnreachableException(),
        };

        return pos >= 0 ? (_position = pos) : throw new IOException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override int Read(Span<byte> buffer)
    {
        var n = _buffer.Length - _position;

        if (n <= 0)
            return 0;

        if (n > buffer.Length)
            n = buffer.Length;

        _buffer.Span.Slice(_position, n).CopyTo(buffer);

        _position += n;

        return n;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    public override int ReadByte()
    {
        var value = (stackalloc byte[1]);

        return Read(value) == 0 ? -1 : value[0];
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled<int>(cancellationToken)
            : new(Read(buffer.Span));
    }

    public override Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (_writable && _position < _buffer.Length && buffer.TryCopyTo(_buffer.Span[_position..]))
            _position += buffer.Length;
        else
            throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Write(buffer.AsSpan(offset, count));
    }

    public override void WriteByte(byte value)
    {
        Write([value]);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled(cancellationToken);

        try
        {
            Write(buffer.Span);
        }
        catch (NotSupportedException ex)
        {
            return ValueTask.FromException(ex);
        }

        return ValueTask.CompletedTask;
    }

    public override Task WriteAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        return WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }
}
