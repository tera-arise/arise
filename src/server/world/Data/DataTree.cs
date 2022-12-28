namespace Arise.Server.World.Data;

public sealed class DataTree : IHostedService
{
    public DataCenterNode Root { get; private set; } = null!;

    private readonly IHostEnvironment _environment;

    public DataTree(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: Add logging.

        await using var stream = EmbeddedDataCenter.OpenStream();
        var attrs = typeof(ThisAssembly).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

        byte[] GetByteArray(string key)
        {
            return Convert.FromHexString(attrs.Single(attr => attr.Key == key).Value!);
        }

        Root = await DataCenter.LoadAsync(
            stream,
            new DataCenterLoadOptions()
                .WithKey(GetByteArray("DataCenterKey"))
                .WithIV(GetByteArray("DataCenterIV"))
                .WithStrict(true)
                .WithLoaderMode(_environment.IsDevelopment() ? DataCenterLoaderMode.Lazy : DataCenterLoaderMode.Eager)
                .WithMutability(DataCenterMutability.Immutable),
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
