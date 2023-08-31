namespace Arise.Tools.Distributor;

internal sealed class DistributorOptions
{
    [Value(0, HelpText = "Path to input TERA directory.")]
    public required DirectoryInfo TeraDirectory { get; init; }

    [Value(1, HelpText = "GitHub personal access token.")]
    public required string GitHubToken { get; init; }

    [Value(2, Default = "tera-arise", HelpText = "GitHub repository owner.")]
    public required string RepositoryOwner { get; init; }

    [Value(3, Default = "arise-client", HelpText = "GitHub repository name.")]
    public required string RepositoryName { get; init; }

    [Option('r', "revision", Default = 387486, HelpText = "TERA client revision.")]
    public required int TeraRevision { get; init; }

    [Option('t', "timeout", HelpText = "GitHub asset upload timeout.")]
    public required TimeSpan UploadTimeout { get; init; }
}
