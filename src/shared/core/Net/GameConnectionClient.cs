using Arise.Bridge;

namespace Arise.Net;

public sealed class GameConnectionClient : GameConnectionManager
{
    private GameConnectionClient(ObjectPool<GameConnectionBuffer> buffers)
        : base(buffers)
    {
    }

    public static GameConnectionClient Create(ObjectPoolProvider objectPoolProvider)
    {
        return new(objectPoolProvider.Create<GameConnectionBuffer>());
    }

    public async ValueTask<GameConnection> ConnectAsync(
        DnsEndPoint endPoint,
        X509Certificate2 authorityCertificate,
        X509Certificate2 clientCertificate,
        CancellationToken cancellationToken = default)
    {
        var clientOptions = new QuicClientConnectionOptions
        {
            RemoteEndPoint = endPoint,
            ClientAuthenticationOptions =
                GameConnectionAuthentication.CreateClientOptions(
                    authorityCertificate, clientCertificate, endPoint.Host),
            MaxInboundUnidirectionalStreams = 1, // Handshake stream.
            MaxInboundBidirectionalStreams = 3, // Prioritized packet streams.
            DefaultCloseErrorCode = CloseErrorCode,
            DefaultStreamErrorCode = StreamErrorCode,
        };

        QuicConnection quicConnection;

        try
        {
            quicConnection = await QuicConnection.ConnectAsync(clientOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (GameConnection.IsNetworkException(ex))
        {
            throw new GameConnectionException("Failed to connect to the remote game server.", ex);
        }

        return await PerformConnectHandshakeAsync(quicConnection, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<GameConnection> PerformConnectHandshakeAsync(
        QuicConnection quicConnection, CancellationToken cancellationToken)
    {
        QuicStream lowPriority;
        QuicStream normalPriority;
        QuicStream highPriority;
        Memory<byte> module;

        try
        {
            var quicStream = await quicConnection.AcceptInboundStreamAsync(cancellationToken).ConfigureAwait(false);

            await using (quicStream.ConfigureAwait(false))
            {
                var accessor = new StreamAccessor(quicStream);

                var size = await accessor.ReadInt32Async(cancellationToken).ConfigureAwait(false);

                // Put an upper limit on the module size to help detect a malformed handshake. It is unlikely that we
                // will ever exceed this, but if we do, just increase the limit.
                if (size is < 0 or > 1024 * 1024)
                    throw new InvalidDataException($"Module size {size} is too large.");

                module = new byte[size];

                await accessor.ReadAsync(module, cancellationToken).ConfigureAwait(false);
            }

            lowPriority = await quicConnection.AcceptInboundStreamAsync(cancellationToken).ConfigureAwait(false);
            normalPriority = await quicConnection.AcceptInboundStreamAsync(cancellationToken).ConfigureAwait(false);
            highPriority = await quicConnection.AcceptInboundStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await quicConnection.DisposeAsync().ConfigureAwait(false);

            if (GameConnection.IsNetworkException(ex) || ex is EndOfStreamException or InvalidDataException)
                throw new GameConnectionException(
                    "Failed to connect to the remote game server due to a handshake failure.", ex);

            throw;
        }

        return await CreateConnectionAsync(
            quicConnection, lowPriority, normalPriority, highPriority, BridgeModuleActivator.Activate(module))
            .ConfigureAwait(false);
    }
}
