namespace Arise.Client.Launcher.Logging;

[RegisterSingleton]
[SuppressMessage("", "CA1812")]
[SuppressMessage("", "CA1848")]
[SuppressMessage("", "CA2254")]
internal sealed class AvaloniaLogSink : ILogSink
{
    private readonly ILoggerFactory _loggerFactory;

    private readonly ConcurrentDictionary<string, Microsoft.Extensions.Logging.ILogger> _loggers = new();

    public AvaloniaLogSink(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public bool IsEnabled(Avalonia.Logging.LogEventLevel level, string area)
    {
        return true;
    }

    public void Log(Avalonia.Logging.LogEventLevel level, string area, object? source, string messageTemplate)
    {
        Log(level, area, source, messageTemplate, []);
    }

    public void Log(
        Avalonia.Logging.LogEventLevel level,
        string area,
        object? source,
        string messageTemplate,
        params object?[] propertyValues)
    {
        var logger = _loggers.GetOrAdd(
            source?.GetType()?.FullName ?? $"Avalonia.{area}", category => _loggerFactory.CreateLogger(category));
        var logLevel = level switch
        {
            Avalonia.Logging.LogEventLevel.Verbose => LogLevel.Trace,
            Avalonia.Logging.LogEventLevel.Debug => LogLevel.Debug,
            Avalonia.Logging.LogEventLevel.Information => LogLevel.Information,
            Avalonia.Logging.LogEventLevel.Warning => LogLevel.Warning,
            Avalonia.Logging.LogEventLevel.Error => LogLevel.Error,
            Avalonia.Logging.LogEventLevel.Fatal => LogLevel.Critical,
            _ => throw new UnreachableException(),
        };

        if (logger.IsEnabled(logLevel))
            logger.Log(logLevel, messageTemplate, propertyValues);
    }
}
