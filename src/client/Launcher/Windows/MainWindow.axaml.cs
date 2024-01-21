using Arise.Client.Launcher.Controllers;

namespace Arise.Client.Launcher.Windows;

public sealed partial class MainWindow : LauncherWindow<MainController>
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        Opened += (_, _) => DataContext.PlayMusic();
        Closed += (_, _) => DataContext.StopMusic();

        base.OnInitialized();
    }
}
