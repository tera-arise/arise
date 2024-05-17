// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Daemon;

internal sealed class DaemonOptions
{
    [Option(
        'e',
        "environment",
        Default = DaemonEnvironment.Development,
        HelpText = "Deployment environment. (Development, Staging, Production)")]
    public required DaemonEnvironment Environment { get; init; }
}
