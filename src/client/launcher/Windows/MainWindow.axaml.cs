namespace Arise.Client.Launcher.Windows;

[RegisterTransient<MainWindow>]
internal sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
