namespace Arise.Server.Web.Net;

[RegisterSingleton<GameDownloadLinks>]
internal sealed class GameDownloadLinks
{
    public Uri TeraManifestUri { get; }

    public Uri TeraDownloadUri { get; }

    public Uri AriseManifestUri { get; }

    public Uri AriseDownloadUri { get; }

    public GameDownloadLinks(IOptions<WebOptions> options)
    {
        static Uri CreateUri(string uri, int revision, string path)
        {
            return new(string.Format(CultureInfo.InvariantCulture, uri, revision, path));
        }

        var value = options.Value;

        var teraRevision = value.TeraRevision;
        var teraFormat = value.TeraDownloadFormat;

        var ariseRevision = ThisAssembly.GameRevision;
        var ariseFormat = value.AriseDownloadFormat;

        TeraManifestUri = CreateUri(teraFormat, teraRevision, "manifest.json");
        TeraDownloadUri = CreateUri(teraFormat, teraRevision, $"TERA.EU.{teraRevision}.{{0}}.zip");
        AriseManifestUri = CreateUri(ariseFormat, ariseRevision, "manifest.json");
        AriseDownloadUri = CreateUri(ariseFormat, ariseRevision, $"TERA.Arise.{ariseRevision}.zip");
    }
}
