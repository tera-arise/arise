namespace Arise.Tools.Patcher;

[SuppressMessage("", "CA1812")]
internal sealed class PatcherOptions
{
    [Value(0, HelpText = "Path to TERA executable.")]
    public required FileInfo Executable { get; init; }
}
