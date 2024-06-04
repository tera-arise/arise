// SPDX-License-Identifier: AGPL-3.0-or-later
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arise.Client.Launcher.Views.Modals;

public partial class RecoverPasswordForm : UserControl
{
    public RecoverPasswordForm()
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
