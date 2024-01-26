namespace Arise.Server.Spatial;

[RegisterSingleton<MapSpatialIndex>]
[SuppressMessage("", "CA1001")]
internal sealed partial class MapSpatialIndex : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Loaded spatial data for zone ({X}, {Y}) in {ElapsedMs:0.0000} ms")]
        public static partial void LoadedSpatialData(ILogger<MapSpatialIndex> logger, int x, int y, double elapsedMs);

        [LoggerMessage(1, LogLevel.Information, "Unloaded spatial data for zone ({X}, {Y})")]
        public static partial void UnloadedSpatialData(ILogger<MapSpatialIndex> logger, int x, int y);
    }

    private sealed class SpatialZone
    {
        public ReadOnlySpan<SpatialVolume> this[int squareX, int squareY, int cellX, int cellY] =>
            _volumes.Span.Slice(
                _volumeIndices[squareX, squareY, cellX, cellY], _volumeCounts[squareX, squareY, cellX, cellY]);

        public required Instant Timestamp { get; set; }

        private readonly int[,,,] _volumeIndices;

        private readonly byte[,,,] _volumeCounts;

        private readonly ReadOnlyMemory<SpatialVolume> _volumes;

        public SpatialZone(int[,,,] volumeIndices, byte[,,,] volumeCounts, ReadOnlyMemory<SpatialVolume> volumes)
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

    private readonly CancellationTokenSource _cts = new();

    private readonly TaskCompletionSource _evictDone = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly ConcurrentDictionary<(int X, int Y), SpatialZone> _zones = new();

    private readonly IFileProvider _fileProvider;

    private readonly IOptions<GameOptions> _options;

    private readonly IClock _clock;

    private readonly ILogger<MapSpatialIndex> _logger;

    public MapSpatialIndex(
        IHostEnvironment environment, IOptions<GameOptions> options, IClock clock, ILogger<MapSpatialIndex> logger)
    {
        _fileProvider = environment.ContentRootFileProvider;
        _options = options;
        _clock = clock;
        _logger = logger;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var ct = _cts.Token;

        _ = Task.Run(() => EvictZonesAsync(ct), ct);

        _ = (object)GetZoneAsync; // TODO

        return Task.CompletedTask;
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        // Signal the evictor task to shut down.
        await _cts.CancelAsync();

        // Note that the evictor task is not expected to encounter any exceptions.
        await _evictDone.Task;

        // The task is done; safe to dispose this now.
        _cts.Dispose();
    }

    private async ValueTask<SpatialZone> GetZoneAsync(int x, int y, CancellationToken cancellationToken)
    {
        if (_zones.TryGetValue((x, y), out var zone))
        {
            zone.Timestamp = _clock.GetCurrentInstant();

            return zone;
        }

        var stopwatch = SlimStopwatch.Create();

        await using var memoryStream = new MemoryStream();

        await using (var fileStream = _fileProvider.GetFileInfo($"zgd/x{x:0000}y{y:0000}.zgd").CreateReadStream())
        await using (var zlibStream = new BrotliStream(fileStream, CompressionMode.Decompress, leaveOpen: true))
            await zlibStream.CopyToAsync(memoryStream, cancellationToken);

        var accessor = new StreamAccessor(memoryStream);

        var volumeIndices = new int[SquaresPerZone, SquaresPerZone, CellsPerSquare, CellsPerSquare];
        var volumeCounts = new byte[SquaresPerZone, SquaresPerZone, CellsPerSquare, CellsPerSquare];
        var volumes = new List<SpatialVolume>(volumeIndices.Length * 4); // Empirically good initial capacity.

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

        zone = new(volumeIndices, volumeCounts, volumes.ToArray())
        {
            Timestamp = _clock.GetCurrentInstant(),
        };

        // We might not have won the race; only log if we did.
        if (_zones.TryAdd((x, y), zone))
            Log.LoadedSpatialData(_logger, x, y, stopwatch.Elapsed.TotalMilliseconds);

        return zone;
    }

    private async Task EvictZonesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // This enumerates a snapshot of the dictionary.
                foreach (var (coords, zone) in _zones)
                {
                    if (_clock.GetCurrentInstant() - zone.Timestamp <= _options.Value.SpatialDataRetentionTime)
                        continue;

                    _ = _zones.TryRemove(coords, out _);

                    Log.UnloadedSpatialData(_logger, coords.X, coords.Y);
                }

                await Task.Delay(_options.Value.SpatialDataPollingTime.ToTimeSpan(), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // StopAsync was called.
        }
        finally
        {
            _evictDone.SetResult();
        }
    }
}
