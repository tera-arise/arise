// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Launcher.Controllers;

[SuppressMessage("", "CA1515")]
public abstract class LauncherController : ObservableObject
{
    public IServiceProvider Services { get; }

    public string Title { get; } = ThisAssembly.GameTitle;

    protected LauncherController(IServiceProvider services)
    {
        Services = services;
    }
}
