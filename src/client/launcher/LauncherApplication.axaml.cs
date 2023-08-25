using Arise.Client.Launcher.Controllers;
using Arise.Client.Launcher.Windows;

namespace Arise.Client.Launcher;

internal sealed partial class LauncherApplication : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Unsafe.As<IClassicDesktopStyleApplicationLifetime>(ApplicationLifetime!).MainWindow = new MainWindow
        {
            DataContext = new MainController(),
        };

        base.OnFrameworkInitializationCompleted();
    }
}
