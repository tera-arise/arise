namespace Arise.Server.Daemon;

[SuppressMessage("", "CA1812")]
internal sealed class DaemonOptions
{
    [Option('e', "environment", HelpText = "Deployment environment. (Development, Staging, Production)")]
    public DaemonEnvironment Environment { get; }

    [Option('s', "services", Default = DaemonServices.All, HelpText = "Services to run. (Web, World)")]
    public DaemonServices Services { get; }

    public DaemonOptions(DaemonEnvironment environment, DaemonServices services)
    {
        Environment = environment;
        Services = services;
    }
}
