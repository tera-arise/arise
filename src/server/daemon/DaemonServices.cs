namespace Arise.Server.Daemon;

[Flags]
internal enum DaemonServices
{
    Web = 0b01,
    World = 0b10,
    All = Web | World,
}
