namespace Arise.Server.Web.Models.Api.Version;

internal sealed class VersionCheckResponse
{
    public required Uri ClientManifestUri { get; init; }

    public required Uri ClientDownloadFormat { get; init; }

    public required Uri AriseManifestUri { get; init; }

    public required Uri AriseDownloadFormat { get; init; }
}
