namespace Arise.Server.Daemon;

// These names must match Microsoft.Extensions.Hosting.Environments.
public enum DaemonEnvironment
{
#if DEBUG
    Development,
#else
    Staging,
    Production,
#endif
}
