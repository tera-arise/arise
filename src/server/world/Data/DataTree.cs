namespace Arise.Server.World.Data;

public sealed partial class DataTree : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Loaded embedded data center as {Mode} in {ElapsedMs:0.0000} ms")]
        public static partial void LoadedDataCenter(ILogger logger, DataCenterLoaderMode mode, double elapsedMs);
    }

    public DataCenterNode Root { get; private set; } = null!;

    private readonly IHostEnvironment _environment;

    private readonly ILogger<DataTree> _logger;

    public DataTree(IHostEnvironment environment, ILogger<DataTree> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var stream = EmbeddedDataCenter.OpenStream();
        var attrs = typeof(ThisAssembly).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

        byte[] GetByteArray(string key)
        {
            return Convert.FromHexString(attrs.Single(attr => attr.Key == key).Value!);
        }

        var mode = _environment.IsDevelopment() ? DataCenterLoaderMode.Lazy : DataCenterLoaderMode.Eager;
        var stamp = Stopwatch.GetTimestamp();

        Root = await DataCenter.LoadAsync(
            stream,
            new DataCenterLoadOptions()
                .WithKey(GetByteArray("DataCenterKey"))
                .WithIV(GetByteArray("DataCenterIV"))
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
