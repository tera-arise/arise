using Arise.Client.Launcher.Controllers;

namespace Arise.Client.Launcher.Windows;

public abstract class LauncherWindow<TController> : Window
    where TController : LauncherController
{
    protected new TController DataContext => Unsafe.As<TController>(base.DataContext!);

    protected LauncherWindow()
    {
    }
}
