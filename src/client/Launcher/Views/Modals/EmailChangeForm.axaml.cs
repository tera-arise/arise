using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arise.Client.Launcher.Views.Modals;

// todo: refactor all this stuff in a proper control
public partial class EmailChangeForm : UserControl
{
    public EmailChangeForm()
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
