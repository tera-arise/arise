namespace Arise.Tools.Patcher;

internal sealed class PatcherOptions
{
    [Value(0, HelpText = "Path to TERA executable.")]
    public required FileInfo Executable { get; init; }
}
