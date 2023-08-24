namespace Arise.Server.Spatial;

[RegisterSingleton]
[SuppressMessage("", "CA1812")]
internal sealed partial class MapSpatialIndex : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Loaded {Count} zone geometry files in {ElapsedMs:0.0000} ms")]
        public static partial void LoadedZoneGeometry(ILogger logger, int Count, double elapsedMs);
    }

    private sealed class SpatialZone
    {
        public ReadOnlySpan<SpatialVolume> this[int squareX, int squareY, int cellX, int cellY] =>
            CollectionsMarshal
                .AsSpan(_volumes)
                .Slice(_volumeIndices[squareX, squareY, cellX, cellY], _volumeCounts[squareX, squareY, cellX, cellY]);

        private readonly int[,,,] _volumeIndices;

        private readonly byte[,,,] _volumeCounts;

        private readonly List<SpatialVolume> _volumes;

        public SpatialZone(int[,,,] volumeIndices, byte[,,,] volumeCounts, List<SpatialVolume> volumes)
        {
            _volumeIndices = volumeIndices;
            _volumeCounts = volumeCounts;
            _volumes = volumes;
        }
    }

    private readonly struct SpatialVolume
    {
        public ushort Z { get; }

        public ushort Height { get; }

        public SpatialVolume(ushort z, ushort height)
        {
            Z = z;
            Height = height;
        }
    }

    private const int SquaresPerZone = 120;

    private const int CellsPerSquare = 8;

    private readonly IHostEnvironment _environment;

    private readonly ILogger<MapSpatialIndex> _logger;

    private FrozenDictionary<(int X, int Y), SpatialZone> _zones = null!;

    public MapSpatialIndex(IHostEnvironment environment, ILogger<MapSpatialIndex> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var stopwatch = SlimStopwatch.Create();

        var zones = new Dictionary<(int, int), SpatialZone>();

        await Parallel.ForEachAsync(
            _environment
                .ContentRootFileProvider
                .GetDirectoryContents("geometry"),
            async (file, ct) =>
            {
                await using var fileStream = file.CreateReadStream();
                await using var zlibStream = new ZLibStream(fileStream, CompressionMode.Decompress, leaveOpen: true);
                await using var bufferedStream = new BufferedStream(zlibStream);

                var accessor = new StreamAccessor(bufferedStream);

                var volumeIndices = new int[SquaresPerZone, SquaresPerZone, CellsPerSquare, CellsPerSquare];
                var volumeCounts = new byte[SquaresPerZone, SquaresPerZone, CellsPerSquare, CellsPerSquare];
                var volumes = new List<SpatialVolume>(volumeIndices.Length * 4);

                var index = 0;
                var lastZ = (ushort)0;
                var lastHeight = (ushort)0;

                for (var sx = 0; sx < SquaresPerZone; sx++)
                {
                    for (var sy = 0; sy < SquaresPerZone; sy++)
                    {
                        for (var cx = 0; cx < CellsPerSquare; cx++)
                        {
                            for (var cy = 0; cy < CellsPerSquare; cy++)
                            {
                                volumeIndices[sx, sy, cx, cy] = index;

                                var count = volumeCounts[sx, sy, cx, cy] = accessor.ReadByte();

                                for (var i = 0; i < count; i++, index++)
                                {
                                    var z = (ushort)(lastZ + accessor.ReadUInt16());
                                    var height = (ushort)(lastHeight + accessor.ReadUInt16());

                                    volumes.Add(new(z, height));

                                    lastZ = z;
                                    lastHeight = height;
                                }
                            }
                        }
                    }
                }

                lock (zones)
                    zones.Add(
                        (int.Parse(file.Name[1..5], CultureInfo.InvariantCulture),
                         int.Parse(file.Name[6..10], CultureInfo.InvariantCulture)),
                        new(volumeIndices, volumeCounts, volumes));
            });

        _zones = zones.ToFrozenDictionary();

        // TODO: Expose methods to perform queries on the data.
        _ = _zones;

        Log.LoadedZoneGeometry(_logger, zones.Count, stopwatch.Elapsed.TotalMilliseconds);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
