using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arise.Client.Launcher.Views.Modals;

public partial class PasswordChangeForm : UserControl
{
    public PasswordChangeForm()
    {
        InitializeComponent();
    }

    // todo: deduplicate this
    private void PasswordTextBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is InputElement s)
        {
            _ = s.Focus();
        }
    }
}
