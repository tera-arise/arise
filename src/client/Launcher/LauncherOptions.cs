// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Launcher;

internal sealed class LauncherOptions : IOptions<LauncherOptions>
{
    public bool GameConsole { get; set; }

    LauncherOptions IOptions<LauncherOptions>.Value => this;
}
