namespace Arise.Server.Data;

public sealed partial class DataGraph : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Loaded embedded data center as {Mode} in {ElapsedMs:0.0000} ms")]
        public static partial void LoadedDataCenter(ILogger logger, DataCenterLoaderMode mode, double elapsedMs);
    }

    public DataCenterNode Root { get; private set; } = null!;

    private readonly IHostEnvironment _environment;

    private readonly ILogger<DataGraph> _logger;

    public DataGraph(IHostEnvironment environment, ILogger<DataGraph> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var attrs = typeof(ThisAssembly).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

        byte[] GetByteArray(string key)
        {
            return Convert.FromHexString(attrs.Single(attr => attr.Key == $"Arise.DataCenter{key}").Value!);
        }

        var mode = _environment.IsDevelopment() ? DataCenterLoaderMode.Lazy : DataCenterLoaderMode.Eager;
        var stamp = Stopwatch.GetTimestamp();

        await using var stream = EmbeddedDataCenter.OpenStream();

        Root = await DataCenter.LoadAsync(
            stream,
            new DataCenterLoadOptions()
                .WithKey(GetByteArray("Key"))
                .WithIV(GetByteArray("IV"))
                .WithStrict(true)
                .WithLoaderMode(mode)
                .WithMutability(DataCenterMutability.Immutable),
            cancellationToken);

        Log.LoadedDataCenter(_logger, mode, Stopwatch.GetElapsedTime(stamp).TotalMilliseconds);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
