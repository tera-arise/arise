namespace Arise.Server.Web.Models.Home;

public sealed record class HomeDownloadModel(
    Uri ClientManifestUri, Uri ClientDownloadUri, Uri AriseManifestUri, Uri AriseDownloadUri);
