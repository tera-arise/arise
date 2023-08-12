namespace Arise.Net;

public sealed class GameConnectionListener : GameConnectionManager
{
    public event Action<GameConnectionListener, IPEndPoint?, GameConnectionException?>? ClientDropped;

    private readonly CancellationTokenSource _cts = new();

    private readonly TaskCompletionSource _acceptDone = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly QuicListener _listener;

    private readonly Func<ReadOnlyMemory<byte>> _moduleProvider;

    private GameConnectionListener(
        ObjectPool<GameConnectionBuffer> buffers, QuicListener listener, Func<ReadOnlyMemory<byte>> moduleProvider)
        : base(buffers)
    {
        _listener = listener;
        _moduleProvider = moduleProvider;

        var ct = _cts.Token;

        _ = Task.Run(() => AcceptClientsAsync(ct), ct);
    }

    private protected override async ValueTask DisposeCoreAsync()
    {
        // Signal the accept task to shut down.
        await _cts.CancelAsync().ConfigureAwait(false);

        // Note that the accept task is not expected to encounter any exceptions.
        await _acceptDone.Task.ConfigureAwait(false);

        // The task is done; safe to dispose this now.
        _cts.Dispose();

        await _listener.DisposeAsync().ConfigureAwait(false);
    }

    public static async ValueTask<GameConnectionListener> CreateAsync(
        IPEndPoint endPoint,
        X509Certificate2 authorityCertificate,
        X509Certificate2 serverCertificate,
        Func<ReadOnlyMemory<byte>> moduleProvider,
        ObjectPoolProvider objectPoolProvider,
        CancellationToken cancellationToken = default)
    {
        var serverOptions = new QuicServerConnectionOptions
        {
            ServerAuthenticationOptions =
                GameConnectionAuthentication.CreateServerOptions(authorityCertificate, serverCertificate),
            MaxInboundUnidirectionalStreams = 0,
            MaxInboundBidirectionalStreams = 0,
            DefaultCloseErrorCode = CloseErrorCode,
            DefaultStreamErrorCode = StreamErrorCode,
        };
        var listenerOptions = new QuicListenerOptions
        {
            ListenEndPoint = endPoint,
            ApplicationProtocols = GameConnectionAuthentication.Protocols,
            ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(serverOptions),
        };

        QuicListener quicListener;

        try
        {
            quicListener = await QuicListener.ListenAsync(listenerOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (GameConnection.IsNetworkException(ex))
        {
            throw new GameConnectionException("Failed to start game connection listener.", ex);
        }

        return new(objectPoolProvider.Create<GameConnectionBuffer>(), quicListener, moduleProvider);
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                QuicConnection quicConnection;

                try
                {
                    quicConnection = await _listener.AcceptConnectionAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (GameConnection.IsNetworkException(ex))
                {
                    var exception = new GameConnectionException(
                        "Dropped incoming game connection due to a network error.", ex);

                    _ = ExceptionDispatchInfo.SetCurrentStackTrace(exception);

                    ClientDropped?.Invoke(this, null, exception);

                    continue;
                }
                catch (OperationCanceledException)
                {
                    // DisposeAsync was called.
                    break;
                }

                // Perform the handshake on the thread pool to avoid slow clients blocking the listener.
                _ = Task.Run(() => PerformAcceptHandshakeAsync(quicConnection, cancellationToken), cancellationToken);
            }
        }
        finally
        {
            _acceptDone.SetResult();
        }
    }

    private async Task PerformAcceptHandshakeAsync(QuicConnection quicConnection, CancellationToken cancellationToken)
    {
        var module = _moduleProvider();

        QuicStream lowPriority;
        QuicStream normalPriority;
        QuicStream highPriority;

        try
        {
            var quicStream = await quicConnection
                .OpenOutboundStreamAsync(QuicStreamType.Unidirectional, cancellationToken)
                .ConfigureAwait(false);

            await using (quicStream.ConfigureAwait(false))
            {
                var accessor = new StreamAccessor(quicStream);

                await accessor.WriteInt32Async(module.Length, cancellationToken).ConfigureAwait(false);
                await accessor.WriteAsync(module, cancellationToken).ConfigureAwait(false);
            }

            lowPriority = await quicConnection
                .OpenOutboundStreamAsync(QuicStreamType.Bidirectional, cancellationToken)
                .ConfigureAwait(false);
            normalPriority = await quicConnection
                .OpenOutboundStreamAsync(QuicStreamType.Bidirectional, cancellationToken)
                .ConfigureAwait(false);
            highPriority = await quicConnection
                .OpenOutboundStreamAsync(QuicStreamType.Bidirectional, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await quicConnection.DisposeAsync().ConfigureAwait(false);

            if (!GameConnection.IsNetworkException(ex))
                throw;

            var exception = new GameConnectionException(
                "Dropped incoming game connection due to a handshake network error.", ex);

            _ = ExceptionDispatchInfo.SetCurrentStackTrace(exception);

            ClientDropped?.Invoke(this, quicConnection.RemoteEndPoint, exception);

            return;
        }

        _ = await CreateConnectionAsync(
            quicConnection,
            lowPriority,
            normalPriority,
            highPriority,
            module).ConfigureAwait(false);
    }
}
