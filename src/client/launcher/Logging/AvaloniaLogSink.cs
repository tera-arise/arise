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

    private static LogLevel TranslateLogLevel(Avalonia.Logging.LogEventLevel level)
    {
        return level switch
        {
            Avalonia.Logging.LogEventLevel.Verbose => LogLevel.Trace,
            Avalonia.Logging.LogEventLevel.Debug => LogLevel.Debug,
            Avalonia.Logging.LogEventLevel.Information => LogLevel.Information,
            Avalonia.Logging.LogEventLevel.Warning => LogLevel.Warning,
            Avalonia.Logging.LogEventLevel.Error => LogLevel.Error,
            Avalonia.Logging.LogEventLevel.Fatal => LogLevel.Critical,
            _ => throw new UnreachableException(),
        };
    }

    private Microsoft.Extensions.Logging.ILogger GetLogger(string area)
    {
        var category = $"Avalonia.{area}";

        return _loggers.GetOrAdd(category, category => _loggerFactory.CreateLogger(category));
    }

    public bool IsEnabled(Avalonia.Logging.LogEventLevel level, string area)
    {
        return GetLogger(area).IsEnabled(TranslateLogLevel(level));
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
        if (IsEnabled(level, area))
            GetLogger(area).Log(TranslateLogLevel(level), messageTemplate, propertyValues);
    }
}
