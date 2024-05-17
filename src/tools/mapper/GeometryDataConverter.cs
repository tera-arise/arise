// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Tools.Mapper;

internal static class GeometryDataConverter
{
    private readonly struct Cell
    {
        public Volume[] Volumes { get; }

        public Cell(int count)
        {
            Volumes = new Volume[count];
        }
    }

    private readonly struct Volume
    {
        public required short Z { get; init; }

        public required ushort Height { get; init; }
    }

    public static async ValueTask ConvertAsync(MapperOptions options)
    {
        var idxFiles = options
            .TopologyDirectory
            .EnumerateFiles("*.idx")
            .ToArray();

        await Terminal.OutLineAsync($"Converting geometry data for {idxFiles.Length} zones...");

        var oldSize = 0L;
        var newSize = 0L;

        await Parallel.ForEachAsync(
            idxFiles,
            async (idxFile, cancellationToken) =>
            {
                var geoFile = new FileInfo(Path.ChangeExtension(idxFile.FullName, "geo"));

                var zoneXY = Path
                    .GetFileNameWithoutExtension(idxFile.Name)
                    .Replace("x", string.Empty, StringComparison.Ordinal)
                    .Replace('y', ' ')
                    .Split()
                    .Select(static coord => int.Parse(coord, CultureInfo.InvariantCulture))
                    .ToArray();

                // TODO: Remove this check when we switch to patch 115.02 data.
                if (!geoFile.Exists)
                {
                    await Terminal.OutLineAsync(
                        $"Missing geometry data for zone ({zoneXY[0]}, {zoneXY[1]}); skipping.", cancellationToken);

                    return;
                }

                _ = Interlocked.Add(ref oldSize, idxFile.Length + geoFile.Length);

                var cells = new Cell[
                    SpatialFacts.SquaresPerZone,
                    SpatialFacts.SquaresPerZone,
                    SpatialFacts.CellsPerSquare,
                    SpatialFacts.CellsPerSquare];

                await using (var idxStream = new BufferedStream(idxFile.OpenRead()))
                await using (var geoStream = new BufferedStream(geoFile.OpenRead()))
                {
                    var idx = new StreamAccessor(idxStream);
                    var geo = new StreamAccessor(geoStream);

                    for (var sx = 0; sx < SpatialFacts.SquaresPerZone; sx++)
                    {
                        for (var sy = 0; sy < SpatialFacts.SquaresPerZone; sy++)
                        {
                            _ = idx.ReadInt32(); // Volume count in this square.

                            for (var cx = 0; cx < SpatialFacts.CellsPerSquare; cx++)
                            {
                                for (var cy = 0; cy < SpatialFacts.CellsPerSquare; cy++)
                                {
                                    var volumes = (cells[sx, sy, cx, cy] = new(count: idx.ReadUInt16())).Volumes;

                                    for (var vi = 0; vi < volumes.Length; vi++)
                                        volumes[vi] = new()
                                        {
                                            Z = geo.ReadInt16(),
                                            Height = geo.ReadUInt16(),
                                        };
                                }
                            }
                        }
                    }
                }

                var zgdFile = new FileInfo(
                    Path.Combine(options.GeometryDirectory.FullName, $"x{zoneXY[0]:0000}y{zoneXY[1]:0000}.zgd"));

                await using (var zgdStream = new BrotliStream(
                    new BufferedStream(zgdFile.Create()), CompressionLevel.SmallestSize))
                {
                    var zgd = new StreamAccessor(zgdStream);

                    var lastZ = (short)0;
                    var lastHeight = (ushort)0;

                    for (var sx = 0; sx < SpatialFacts.SquaresPerZone; sx++)
                    {
                        for (var sy = 0; sy < SpatialFacts.SquaresPerZone; sy++)
                        {
                            for (var cx = 0; cx < SpatialFacts.CellsPerSquare; cx++)
                            {
                                for (var cy = 0; cy < SpatialFacts.CellsPerSquare; cy++)
                                {
                                    var cell = cells[sx, sy, cx, cy];

                                    // Empirically, this range constraint has held for every zone thus far.
                                    zgd.WriteByte(checked((byte)cell.Volumes.Length));

                                    foreach (var volume in cell.Volumes)
                                    {
                                        // Improve compression by writing differences rather than actual values.
                                        zgd.WriteInt16((short)(volume.Z - lastZ));
                                        zgd.WriteUInt16((ushort)(volume.Height - lastHeight));

                                        lastZ = volume.Z;
                                        lastHeight = volume.Height;
                                    }
                                }
                            }
                        }
                    }
                }

                zgdFile.Refresh();

                _ = Interlocked.Add(ref newSize, zgdFile.Length);
            });

        await Terminal.OutLineAsync(
            $"Achieved a {((decimal)newSize - oldSize) / oldSize:P2} geometry data size difference.");
    }
}
