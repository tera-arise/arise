// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Launcher.Controllers;

namespace Arise.Client.Launcher.Windows;

[SuppressMessage("", "CA1515")]
public abstract class LauncherWindow<TController> : Window
    where TController : LauncherController
{
    protected new TController DataContext => Unsafe.As<TController>(base.DataContext!);

    protected LauncherWindow()
    {
    }
}
