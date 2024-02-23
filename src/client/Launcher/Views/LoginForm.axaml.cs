using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arise.Client.Launcher.Views;

public partial class LoginForm : UserControl
{
    public LoginForm()
    {
        InitializeComponent();
    }

    private void EmailTextBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is InputElement s)
        {
            _ = s.Focus();
        }
    }
}
