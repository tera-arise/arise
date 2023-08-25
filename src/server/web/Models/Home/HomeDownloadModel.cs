namespace Arise.Server.Web.Models.Home;

internal sealed class HomeDownloadModel
{
    public required Uri TeraManifestUri { get; init; }

    public required Uri TeraDownloadUri { get; init; }

    public required Uri AriseManifestUri { get; init; }

    public required Uri AriseDownloadUri { get; init; }
}
