namespace Arise.Gateway;

public sealed class LauncherHelloResponse
{
    public required Uri? NewsUri { get; init; }

    public required Uri TeraManifestUri { get; init; }

    public required Uri TeraDownloadFormat { get; init; }

    public required Uri AriseManifestUri { get; init; }

    public required Uri AriseDownloadFormat { get; init; }
}
