namespace Arise.IO;

public sealed class SlimMemoryStream : Stream
{
    public Memory<byte> Buffer
    {
        get => _buffer;
        set
        {
            _buffer = value;
            _position = 0;
        }
    }

    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override bool CanSeek => true;

    public override long Length => Buffer.Length;

    public override long Position
    {
        get => _position;
        set => _position = (int)value;
    }

    private Memory<byte> _buffer;

    private int _position;

    public override long Seek(long offset, SeekOrigin origin)
    {
        var pos = origin switch
        {
            SeekOrigin.Begin => (int)offset,
            SeekOrigin.Current => _position + (int)offset,
            SeekOrigin.End => Buffer.Length + (int)offset,
            _ => throw new UnreachableException(),
        };

        return pos >= 0 ? (_position = pos) : throw new IOException();
    }

    public override void SetLength(long value)
    {
        Buffer = Buffer[..(int)value];
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
        var n = Buffer.Length - _position;

        if (n <= 0)
            return 0;

        if (n > buffer.Length)
            n = buffer.Length;

        Buffer.Span.Slice(_position, n).CopyTo(buffer);

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
        if (_position < Buffer.Length && buffer.TryCopyTo(Buffer.Span[_position..]))
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
        // TODO: Use collection expression.
        Write(stackalloc[] { value });
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
