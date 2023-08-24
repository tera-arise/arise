namespace Arise.Server.Daemon;

[SuppressMessage("", "CA1812")]
internal sealed class DaemonOptions
{
    [Option(
        'e',
        "environment",
        Default = DaemonEnvironment.Development,
        HelpText = "Deployment environment. (Development, Staging, Production)")]
    public required DaemonEnvironment Environment { get; init; }

    [Option('s', "services", Default = DaemonServices.All, HelpText = "Services to run. (Web, World)")]
    public required DaemonServices Services { get; init; }
}
