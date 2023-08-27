using Arise.Client.Launcher.Controllers;

namespace Arise.Client.Launcher.Windows;

[RegisterTransient<MainWindow>]
internal sealed partial class MainWindow : LauncherWindow<MainController>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
