namespace Arise.Server.Web.Net;

[RegisterSingleton]
internal sealed class GameDownloadLinks
{
    public Uri TeraManifestUri { get; }

    public Uri TeraDownloadUri { get; }

    public Uri AriseManifestUri { get; }

    public Uri AriseDownloadUri { get; }

    public GameDownloadLinks(IOptions<WebOptions> options)
    {
        static (Uri, Uri) GetUris(string repository, string version, string format)
        {
            var uri = $"https://github.com/tera-arise/arise-{repository}/releases/{version}/download/";

            return (new($"{uri}manifest.json"), new($"{uri}{format}"));
        }

        var teraRevision = options.Value.ClientRevision;
        var ariseRevision = int.Parse(
            typeof(ThisAssembly)
                .Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(static attr => attr.Key == "Arise.GameRevision")
                .Value!,
            CultureInfo.InvariantCulture);

        (TeraManifestUri, TeraDownloadUri) = GetUris(
            "client", $"r{teraRevision}", $"TERA.EU.{teraRevision}.{{0}}.zip");
        (AriseManifestUri, AriseDownloadUri) = GetUris(
            "release", $"v{ariseRevision}", $"TERA.Arise.{ariseRevision}.zip");
    }
}
