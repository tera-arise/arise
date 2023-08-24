namespace Arise.Server.Web.Services;

[RegisterSingleton]
public sealed class GameDownloadProvider
{
    public Uri ClientManifestUri { get; }

    public Uri ClientDownloadUri { get; }

    public Uri AriseManifestUri { get; }

    public Uri AriseDownloadUri { get; }

    private static readonly int _version =
        int.Parse(
            typeof(ThisAssembly)
                .Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(static attr => attr.Key == "Arise.GameRevision")
                .Value!,
            CultureInfo.InvariantCulture);

    public GameDownloadProvider(IOptions<WebOptions> options)
    {
        static (Uri, Uri) GetUris(string repository, string version, string format)
        {
            var uri = $"https://github.com/tera-arise/arise-{repository}/releases/{version}/download/";

            return (new($"{uri}manifest.json"), new($"{uri}{format}"));
        }

        var revision = options.Value.ClientRevision;

        (ClientManifestUri, ClientDownloadUri) = GetUris("client", $"r{revision}", $"TERA.EU.{revision}.{{0}}.zip");
        (AriseManifestUri, AriseDownloadUri) = GetUris("release", $"v{_version}", $"TERA.Arise.{_version}.zip");
    }
}
