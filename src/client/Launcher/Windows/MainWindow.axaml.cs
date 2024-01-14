// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Launcher.Controllers;
using Avalonia.Interactivity;

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

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode)
            return;

        // todo: check if something needs to be disposed beforehand
        // todo: check if this should just be minimized to tray instead of closing
        Close();
    }

    private void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}
