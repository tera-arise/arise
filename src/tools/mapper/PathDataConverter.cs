// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Tools.Mapper;

internal static class PathDataConverter
{
    private readonly struct Zone
    {
        public required int X { get; init; }

        public required int Y { get; init; }

        public Square[,] Squares { get; } = new Square[SpatialFacts.SquaresPerZone, SpatialFacts.SquaresPerZone];

        public Zone()
        {
        }
    }

    private readonly struct Square
    {
        public int Index { get; init; }

        public required int Count { get; init; }
    }

    private readonly struct Node
    {
        public required Vector3 Position { get; init; }

        public int[] Edges { get; } = new int[SpatialFacts.EdgesPerNode];

        public Node()
        {
        }
    }

    [SuppressMessage("", "CA1308")]
    public static async ValueTask ConvertAsync(MapperOptions options)
    {
        var gdiFiles = options
            .TopologyDirectory
            .EnumerateFiles("*.gdi")
            .ToArray();

        await Terminal.OutLineAsync($"Converting pathing data for {gdiFiles.Length} areas...");

        var oldSize = 0L;
        var newSize = 0L;

        await Parallel.ForEachAsync(
            gdiFiles,
            async (gdiFile, cancellationToken) =>
            {
                var nodFile = new FileInfo(Path.ChangeExtension(gdiFile.FullName, "nod"));

                _ = Interlocked.Add(ref oldSize, gdiFile.Length + nodFile.Length);

                Node[] nodes;
                Zone[,] zones;

                await using (var gdiStream = new BufferedStream(gdiFile.OpenRead()))
                {
                    var gdi = new StreamAccessor(gdiStream);

                    var (firstZoneX, firstZoneY) = (gdi.ReadInt32(), gdi.ReadInt32());

                    var zonesX = gdi.ReadInt32() - firstZoneX + 1;
                    var zonesY = gdi.ReadInt32() - firstZoneY + 1;

                    nodes = new Node[gdi.ReadInt32()];

                    await using (var nodStream = new BufferedStream(nodFile.OpenRead()))
                    {
                        var nod = new StreamAccessor(nodStream);

                        for (var ni = 0; ni < nodes.Length; ni++)
                        {
                            var edges = (nodes[ni] = new()
                            {
                                Position = nod.ReadVector3(),
                            }).Edges;

                            for (var ei = 0; ei < SpatialFacts.EdgesPerNode; ei++)
                                edges[ei] = nod.ReadInt32();

                            for (var ei = 0; ei < SpatialFacts.EdgesPerNode; ei++)
                                _ = nod.ReadInt32(); // Distance along this edge.
                        }
                    }

                    zones = new Zone[zonesX, zonesY];

                    for (var zx = 0; zx < zonesX; zx++)
                    {
                        for (var zy = 0; zy < zonesY; zy++)
                        {
                            var squares = (zones[zx, zy] = new()
                            {
                                X = firstZoneX + zx,
                                Y = firstZoneY + zy,
                            }).Squares;

                            for (var sx = 0; sx < SpatialFacts.SquaresPerZone; sx++)
                                for (var sy = 0; sy < SpatialFacts.SquaresPerZone; sy++)
                                    squares[sx, sy] = new()
                                    {
                                        Count = gdi.ReadUInt16(),
                                    };
                        }
                    }

                    for (var zx = 0; zx < zonesX; zx++)
                    {
                        for (var zy = 0; zy < zonesY; zy++)
                        {
                            var squares = zones[zx, zy].Squares;

                            for (var sx = 0; sx < SpatialFacts.SquaresPerZone; sx++)
                                for (var sy = 0; sy < SpatialFacts.SquaresPerZone; sy++)
                                    squares[sx, sy] = new()
                                    {
                                        Index = gdi.ReadInt32(),
                                        Count = squares[sx, sy].Count,
                                    };
                        }
                    }
                }

                var apdFile = new FileInfo(
                    Path.Combine(
                        options.GeometryDirectory.FullName,
                        Path.ChangeExtension(gdiFile.Name["pathdata_".Length..].ToLowerInvariant(), "apd")));

                await using (var apdStream = new BrotliStream(
                    new BufferedStream(apdFile.Create()), CompressionLevel.SmallestSize))
                {
                    var apd = new StreamAccessor(apdStream);

                    apd.WriteInt32(nodes.Length);

                    foreach (var node in nodes)
                    {
                        apd.WriteVector3(node.Position);

                        var directions = (byte)0;

                        for (var ei = 0; ei < SpatialFacts.EdgesPerNode; ei++)
                            if (node.Edges[ei] != -1)
                                directions |= (byte)(1 << ei);

                        apd.WriteByte(directions);

                        foreach (var edge in node.Edges)
                            if (edge != -1)
                                apd.WriteInt32(edge);
                    }

                    apd.WriteInt32(zones.Length);

                    foreach (var zone in zones)
                    {
                        apd.WriteInt32(zone.X);
                        apd.WriteInt32(zone.Y);

                        for (var sx = 0; sx < SpatialFacts.SquaresPerZone; sx++)
                        {
                            for (var sy = 0; sy < SpatialFacts.SquaresPerZone; sy++)
                            {
                                var square = zone.Squares[sx, sy];

                                apd.WriteInt32(square.Index);

                                // Empirically, this range constraint has held for every square thus far.
                                apd.WriteByte(checked((byte)square.Count));
                            }
                        }
                    }
                }

                apdFile.Refresh();

                _ = Interlocked.Add(ref newSize, apdFile.Length);
            });

        await Terminal.OutLineAsync(
            $"Achieved a {((decimal)newSize - oldSize) / oldSize:P2} path data size difference.");
    }
}
