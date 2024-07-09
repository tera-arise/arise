using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arise.Client.Launcher.Views.Modals;

public partial class AccountVerificationForm : UserControl
{
    public AccountVerificationForm()
    {
        InitializeComponent();
    }

    private void TokenTextBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is InputElement i)
        {
            _ = i.Focus();
        }
    }
}
