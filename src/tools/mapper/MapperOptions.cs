namespace Arise.Tools.Mapper;

[SuppressMessage("", "CA1812")]
internal sealed class MapperOptions
{
    [Value(0, HelpText = "Path to TERA directory.")]
    public required DirectoryInfo GameDirectory { get; init; }

    [Value(0, HelpText = "Path to output directory.")]
    public required DirectoryInfo OutputDirectory { get; init; }
}