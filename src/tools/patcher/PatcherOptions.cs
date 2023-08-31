namespace Arise.Tools.Patcher;

internal sealed class PatcherOptions
{
    [Value(0, HelpText = "Path to input/output TERA executable.")]
    public required FileInfo TeraExecutableFile { get; init; }
}
