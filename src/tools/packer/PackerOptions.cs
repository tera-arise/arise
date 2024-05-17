// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Tools.Packer;

internal sealed class PackerOptions
{
    [Value(0, HelpText = "Path to input data directory.")]
    public required DirectoryInfo DataDirectory { get; init; }

    [Value(1, HelpText = "Path to output data center file.")]
    public required FileInfo DataCenterFile { get; init; }

    [Value(2, HelpText = "Data center revision.")]
    public required int DataCenterRevision { get; init; }

    [Value(3, HelpText = "Data center encryption key.")]
    public required string EncryptionKey { get; init; }

    [Value(4, HelpText = "Data center encryption IV.")]
    public required string EncryptionIV { get; init; }
}
