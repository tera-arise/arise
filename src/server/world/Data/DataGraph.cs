namespace Arise.Server.Data;

[RegisterSingleton<DataGraph>]
internal sealed partial class DataGraph : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Loaded embedded data center as {Mode} in {ElapsedMs:0.0000} ms")]
        public static partial void LoadedDataCenter(
            ILogger<DataGraph> logger, DataCenterLoaderMode mode, double elapsedMs);
    }

    public DataCenterNode Root { get; private set; } = null!;

    private readonly IHostEnvironment _environment;

    private readonly ILogger<DataGraph> _logger;

    public DataGraph(IHostEnvironment environment, ILogger<DataGraph> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var stopwatch = SlimStopwatch.Create();

        await using var stream = EmbeddedDataCenter.OpenStream();

        var mode = _environment.IsDevelopment() ? DataCenterLoaderMode.Lazy : DataCenterLoaderMode.Eager;

        Root = await DataCenter.LoadAsync(
            stream,
            new DataCenterLoadOptions()
                .WithKey(DataCenterParameters.Key.Span)
                .WithIV(DataCenterParameters.IV.Span)
                .WithStrict(true)
                .WithLoaderMode(mode)
                .WithMutability(DataCenterMutability.Immutable),
            cancellationToken);

        Log.LoadedDataCenter(_logger, mode, stopwatch.Elapsed.TotalMilliseconds);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
