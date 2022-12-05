namespace Arise.Tools.Distributor;

[SuppressMessage("", "CA1812")]
internal sealed class DistributorOptions
{
    [Value(0, HelpText = "Path to TERA directory.")]
    public DirectoryInfo Directory { get; }

    [Value(1, HelpText = "GitHub personal access token.")]
    public string Token { get; }

    [Option('r', "revision", Default = 387486, HelpText = "TERA client revision.")]
    public int Revision { get; }

    [Option('t', "timeout", HelpText = "GitHub asset upload timeout.")]
    public TimeSpan Timeout { get; }

    public DistributorOptions(DirectoryInfo directory, string token, int revision, TimeSpan timeout)
    {
        Directory = directory;
        Token = token;
        Revision = revision;
        Timeout = timeout != TimeSpan.Zero ? timeout : TimeSpan.FromHours(1);
    }
}
