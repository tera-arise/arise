namespace Arise.Tools.Distributor;

internal sealed class DistributorOptions
{
    [Value(0, HelpText = "Path to TERA directory.")]
    public required DirectoryInfo Directory { get; init; }

    [Value(1, HelpText = "GitHub personal access token.")]
    public required string Token { get; init; }

    [Option('r', "revision", Default = 387486, HelpText = "TERA client revision.")]
    public required int Revision { get; init; }

    [Option('t', "timeout", HelpText = "GitHub asset upload timeout.")]
    public required TimeSpan Timeout { get; init; }
}
