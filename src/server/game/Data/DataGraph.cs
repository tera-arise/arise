namespace Arise.Server.Data;

[RegisterSingleton<DataGraph>]
internal sealed partial class DataGraph : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Loaded embedded data center in {ElapsedMs:0.0000} ms")]
        public static partial void LoadedDataCenter(ILogger<DataGraph> logger, double elapsedMs);
    }

    public DataCenterNode Root { get; private set; } = null!;

    private readonly ILogger<DataGraph> _logger;

    public DataGraph(ILogger<DataGraph> logger)
    {
        _logger = logger;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var stopwatch = SlimStopwatch.Create();

        await using var stream = EmbeddedDataCenter.OpenStream();

        Root = await DataCenter.LoadAsync(
            stream,
            new DataCenterLoadOptions()
                .WithKey(ThisAssembly.DataCenterKey.Span)
                .WithIV(ThisAssembly.DataCenterIV.Span)
                .WithStrict(true)
                .WithMutability(DataCenterMutability.Immutable),
            cancellationToken);

        Log.LoadedDataCenter(_logger, stopwatch.Elapsed.TotalMilliseconds);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
