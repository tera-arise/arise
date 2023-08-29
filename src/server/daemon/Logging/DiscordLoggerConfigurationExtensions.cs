namespace Arise.Server.Daemon.Logging;

internal static class DiscordLoggerConfigurationExtensions
{
    [SuppressMessage("", "CA2000")]
    public static LoggerConfiguration Discord(
        this LoggerSinkConfiguration sinkConfiguration,
        string botToken,
        ulong guildId,
        ulong channelId,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
    {
        return sinkConfiguration.Sink(new DiscordSink(botToken, guildId, channelId), restrictedToMinimumLevel);
    }
}
