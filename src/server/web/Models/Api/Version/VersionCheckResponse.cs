namespace Arise.Server.Web.Models.Api.Version;

internal sealed class VersionCheckResponse
{
    public required Uri TeraManifestUri { get; init; }

    public required Uri TeraDownloadFormat { get; init; }

    public required Uri AriseManifestUri { get; init; }

    public required Uri AriseDownloadFormat { get; init; }
}
