using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arise.Client.Launcher.Views;

public partial class RegistrationForm : UserControl
{
    public RegistrationForm()
    {
        InitializeComponent();
    }

    private void EmailTextBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is InputElement i)
        {
            _ = i.Focus();
        }
    }
}
