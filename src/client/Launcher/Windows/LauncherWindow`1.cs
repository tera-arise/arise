using Arise.Client.Launcher.Controllers;

namespace Arise.Client.Launcher.Windows;

public abstract class LauncherWindow<TController> : ReactiveWindow<TController>
    where TController : LauncherController
{
    protected LauncherWindow()
    {
        _ = this.WhenActivated(static _ => { });
    }
}
