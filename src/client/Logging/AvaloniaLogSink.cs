namespace Arise.Client.Logging;

[SuppressMessage("", "CA1848")]
[SuppressMessage("", "CA2254")]
internal sealed class AvaloniaLogSink : ILogSink
{
    private readonly ILoggerFactory _loggerFactory;

    private readonly ConcurrentDictionary<string, ILogger> _loggers = new();

    public AvaloniaLogSink(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public bool IsEnabled(LogEventLevel level, string area)
    {
        return true;
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        Log(level, area, source, messageTemplate, []);
    }

    public void Log(
        LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        var logger = _loggers.GetOrAdd(
            source?.GetType()?.FullName ?? $"Avalonia.{area}",
            static (category, factory) => factory.CreateLogger(category),
            _loggerFactory);
        var logLevel = level switch
        {
            LogEventLevel.Verbose => LogLevel.Trace,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => throw new UnreachableException(),
        };

        if (logger.IsEnabled(logLevel))
            logger.Log(logLevel, messageTemplate, propertyValues);
    }
}
