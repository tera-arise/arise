namespace Arise.Tools.Mapper;

internal sealed class MapperOptions
{
    [Value(0, HelpText = "Path to input TERA directory.")]
    public required DirectoryInfo TeraDirectory { get; init; }

    [Value(1, HelpText = "Path to output geometry directory.")]
    public required DirectoryInfo GeometryDirectory { get; init; }
}
