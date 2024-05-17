// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Tools.Patcher;

internal sealed class PatcherOptions
{
    [Value(0, HelpText = "Path to input original TERA executable.")]
    public required FileInfo OriginalTeraExecutableFile { get; init; }

    [Value(1, HelpText = "Path to output patched TERA executable.")]
    public required FileInfo PatchedTeraExecutableFile { get; init; }
}
