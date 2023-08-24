namespace Arise.Server.Web.Models.Home;

internal sealed class HomeDownloadModel
{
    public required Uri ClientManifestUri { get; init; }

    public required Uri ClientDownloadUri { get; init; }

    public required Uri AriseManifestUri { get; init; }

    public required Uri AriseDownloadUri { get; init; }
}
