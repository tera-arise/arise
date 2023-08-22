using Arise.Net.Packets;

namespace Arise.Net;

[SuppressMessage("", "CA1001")]
public sealed class GameConnectionConduit
{
    // TODO: https://github.com/dotnet/runtime/issues/90281

    public GameConnection Connection { get; }

    private readonly CancellationTokenSource _cts = new();

    private readonly TaskCompletionSource _receiveDone = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly TaskCompletionSource _sendDone = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Channel<(GameConnectionBuffer, TaskCompletionSource<bool>?)> _sendQueue =
        Channel.CreateUnbounded<(GameConnectionBuffer, TaskCompletionSource<bool>?)>(new()
        {
            SingleReader = true,
        });

    private readonly QuicStream _stream;

    internal GameConnectionConduit(GameConnection connection, QuicStream stream, Task ready)
    {
        Connection = connection;
        _stream = stream;

        var ct = _cts.Token;

        _ = Task.Run(() => ReceivePacketsAsync(ready, ct), ct);
        _ = Task.Run(() => SendPacketsAsync(ready, ct), ct);
    }

    internal async ValueTask DisposeAsync()
    {
        _sendQueue.Writer.Complete();

        // Signal the receive/send tasks to shut down.
        await _cts.CancelAsync().ConfigureAwait(false);

        try
        {
            // The tasks may encounter network errors that we bubble up to GameConnection.DisposeAsync.
            await Task
                .WhenAll(_receiveDone.Task, _sendDone.Task)
                .PreserveAggregateException()
                .ConfigureAwait(false);
        }
        finally
        {
            // Tasks are gone; safe to dispose this now.
            _cts.Dispose();

            await foreach (var (buffer, tcs) in _sendQueue.Reader.ReadAllAsync())
            {
                tcs?.SetResult(false);

                Connection.Manager.Buffers.Return(buffer);
            }

            await _stream.DisposeAsync().ConfigureAwait(false);
        }
    }

    public void PostPacket(GamePacket packet)
    {
        _ = TryPostPacket(packet);
    }

    public bool TryPostPacket(GamePacket packet)
    {
        return TryWritePacket(packet, completion: null);
    }

    public void PostPacket(TeraGamePacketCode code, ReadOnlySpan<byte> payload)
    {
        _ = TryPostPacket(code, payload);
    }

    public void PostPacket(AriseGamePacketCode code, ReadOnlySpan<byte> payload)
    {
        _ = TryPostPacket(code, payload);
    }

    public bool TryPostPacket(TeraGamePacketCode code, ReadOnlySpan<byte> payload)
    {
        return TryWritePacket(GameConnectionChannel.Tera, (ushort)code, payload, completion: null);
    }

    public bool TryPostPacket(AriseGamePacketCode code, ReadOnlySpan<byte> payload)
    {
        return TryWritePacket(GameConnectionChannel.Arise, (ushort)code, payload, completion: null);
    }

    public async ValueTask SendPacketAsync(GamePacket packet)
    {
        _ = await TrySendPacketAsync(packet).ConfigureAwait(false);
    }

    public async ValueTask<bool> TrySendPacketAsync(GamePacket packet)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        return TryWritePacket(packet, tcs) && await tcs.Task.ConfigureAwait(false);
    }

    public async ValueTask SendPacketAsync(TeraGamePacketCode code, ReadOnlyMemory<byte> payload)
    {
        _ = await TrySendPacketAsync(code, payload).ConfigureAwait(false);
    }

    public async ValueTask SendPacketAsync(AriseGamePacketCode code, ReadOnlyMemory<byte> payload)
    {
        _ = await TrySendPacketAsync(code, payload).ConfigureAwait(false);
    }

    public async ValueTask<bool> TrySendPacketAsync(TeraGamePacketCode code, ReadOnlyMemory<byte> payload)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        return
            TryWritePacket(GameConnectionChannel.Tera, (ushort)code, payload.Span, tcs) &&
            await tcs.Task.ConfigureAwait(false);
    }

    public async ValueTask<bool> TrySendPacketAsync(AriseGamePacketCode code, ReadOnlyMemory<byte> payload)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        return
            TryWritePacket(GameConnectionChannel.Arise, (ushort)code, payload.Span, tcs) &&
            await tcs.Task.ConfigureAwait(false);
    }

    private bool TryWritePacket(
        GameConnectionChannel channel, ushort code, ReadOnlySpan<byte> payload, TaskCompletionSource<bool>? completion)
    {
        var buffer = Connection.Manager.Buffers.Get();

        buffer.Channel = channel;
        buffer.Length = (ushort)payload.Length;
        buffer.Code = code;

        buffer.ConvertToSession(Connection.Module.Protocol);

        payload.CopyTo(buffer.Payload.Span);

        return _sendQueue.Writer.TryWrite((buffer, completion));
    }

    private bool TryWritePacket(GamePacket packet, TaskCompletionSource<bool>? completion)
    {
        var buffer = Connection.Manager.Buffers.Get();

        buffer.ResetStream(length: null);

        var accessor = buffer.PayloadAccessor;

        packet.Serialize(accessor);

        buffer.Channel = packet.Channel;
        buffer.Length = (ushort)accessor.Position;
        buffer.Code = packet.RawCode;

        buffer.ConvertToSession(Connection.Module.Protocol);

        return _sendQueue.Writer.TryWrite((buffer, completion));
    }

    private async Task ReceivePacketsAsync(Task ready, CancellationToken cancellationToken)
    {
        await ready.ConfigureAwait(false);

        var manager = Connection.Manager;
        var buffers = manager.Buffers;

        // We can use a single buffer for receiving packets for the lifetime of the connection. This works out because
        // the raw packet events on GameConnection are not expected to keep the payload buffer around after they return,
        // and the typed packet events do not get a reference to the buffer at all.
        var buffer = buffers.Get();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _stream.ReadExactlyAsync(buffer.Header, cancellationToken).ConfigureAwait(false);

                if (!buffer.TryConvertToReal(Connection.Module.Protocol))
                    throw new InvalidDataException();

                await _stream.ReadExactlyAsync(buffer.Payload, cancellationToken).ConfigureAwait(false);

                manager.HandleReceivedPacket(this, buffer);
            }
        }
        catch (Exception ex)
        {
            Connection.InternalDispose();

            if (GameConnection.IsNetworkException(ex) || ex is InvalidDataException)
            {
                if (ex is not QuicException qex || !GameConnection.IsInnocuousError(qex.QuicError))
                    _sendDone.SetException(ex);
            }
            else if (ex is not (EndOfStreamException or OperationCanceledException))
                throw;
        }
        finally
        {
            buffers.Return(buffer);

            _ = _receiveDone.TrySetResult();
        }
    }

    private async Task SendPacketsAsync(Task ready, CancellationToken cancellationToken)
    {
        var manager = Connection.Manager;

        await ready.ConfigureAwait(false);

        try
        {
            await foreach (var (buffer, tcs) in _sendQueue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await _stream.WriteAsync(buffer.Packet, cancellationToken).ConfigureAwait(false);

                    manager.HandleSentPacket(this, buffer);
                }
                catch
                {
                    tcs?.SetResult(false);

                    throw;
                }
                finally
                {
                    manager.Buffers.Return(buffer);
                }

                tcs?.SetResult(true);
            }
        }
        catch (Exception ex)
        {
            Connection.InternalDispose();

            if (GameConnection.IsNetworkException(ex) || ex is InvalidDataException)
            {
                if (ex is not QuicException qex || !GameConnection.IsInnocuousError(qex.QuicError))
                    _sendDone.SetException(ex);
            }
            else if (ex is not OperationCanceledException)
                throw;
        }
        finally
        {
            _ = _sendDone.TrySetResult();
        }
    }
}
