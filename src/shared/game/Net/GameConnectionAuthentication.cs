namespace Arise.Net;

internal static class GameConnectionAuthentication
{
    public static SslApplicationProtocol Protocol { get; } = new("arise");

    // TODO: https://github.com/dotnet/runtime/issues/87270

    private static readonly Oid _subjectKeyIdentifier = new("2.5.29.14");

    private static readonly Oid _keyUsage = new("2.5.29.15");

    private static readonly Oid _subjectAltName = new("2.5.29.17");

    private static readonly Oid _basicConstraints = new("2.5.29.19");

    private static readonly Oid _authorityKeyIdentifier = new("2.5.29.35");

    private static readonly Oid _extKeyUsage = new("2.5.29.37");

    private static readonly Oid _serverAuth = new("1.3.6.1.5.5.7.3.1");

    private static readonly Oid _clientAuth = new("1.3.6.1.5.5.7.3.2");

    [SuppressMessage("", "CA2000")]
    public static SslClientAuthenticationOptions CreateClientOptions(
        X509Certificate2 authorityCertificate, X509Certificate2 clientCertificate, string hostName)
    {
        return new()
        {
            TargetHost = hostName,
            ApplicationProtocols = [Protocol],
            CertificateChainPolicy = ConfigureChainPolicy(authorityCertificate, _serverAuth),
            ClientCertificates = [new(clientCertificate)],
            RemoteCertificateValidationCallback = static (_, _, _, errs) => errs == SslPolicyErrors.None,
        };
    }

    public static SslServerAuthenticationOptions CreateServerOptions(
        X509Certificate2 authorityCertificate, X509Certificate2 serverCertificate)
    {
        return new()
        {
            ApplicationProtocols = [Protocol],
            CertificateChainPolicy = ConfigureChainPolicy(authorityCertificate, _clientAuth),
            ServerCertificate = serverCertificate,
            ClientCertificateRequired = true,
            RemoteCertificateValidationCallback = static (_, _, _, errs) => errs == SslPolicyErrors.None,
        };
    }

    private static X509ChainPolicy ConfigureChainPolicy(X509Certificate2 authorityCertificate, Oid eku)
    {
        return new()
        {
            CertificatePolicy =
            {
                _subjectKeyIdentifier,
                _keyUsage,
                _subjectAltName,
                _basicConstraints,
                _authorityKeyIdentifier,
                _extKeyUsage,
            },
            ApplicationPolicy =
            {
                eku,
            },
            CustomTrustStore =
            {
                new(authorityCertificate),
            },
            TrustMode = X509ChainTrustMode.CustomRootTrust,
            DisableCertificateDownloads = true,
            RevocationMode = X509RevocationMode.NoCheck,
        };
    }
}
