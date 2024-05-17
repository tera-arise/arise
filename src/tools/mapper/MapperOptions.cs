// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Tools.Mapper;

internal sealed class MapperOptions
{
    [Value(0, HelpText = "Path to input topology directory.")]
    public required DirectoryInfo TopologyDirectory { get; init; }

    [Value(1, HelpText = "Path to output geometry directory.")]
    public required DirectoryInfo GeometryDirectory { get; init; }
}
