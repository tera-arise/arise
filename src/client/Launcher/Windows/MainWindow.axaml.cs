// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Launcher.Controllers;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Arise.Client.Launcher.Windows;

internal sealed partial class MainWindow : LauncherWindow<MainController>
{
    public MainWindow()
    {
        InitializeComponent();

        // RendererDiagnostics.DebugOverlays = Avalonia.Rendering.RendererDebugOverlays.LayoutTimeGraph | Avalonia.Rendering.RendererDebugOverlays.RenderTimeGraph | Avalonia.Rendering.RendererDebugOverlays.Fps;
    }

    protected override void OnInitialized()
    {
        if (!Design.IsDesignMode)
        {
            Closed += (_, _) => DataContext.StopMusic(); // todo: this might be replaced by a message, if more thing will have to be handled when window is closed
        }

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

    // failed attempt to optimize modal background transition
    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        var sw = Stopwatch.StartNew();

        var rtb = new RenderTargetBitmap(new PixelSize((int)Root.Bounds.Width, (int)Root.Bounds.Height));
        rtb.Render(Root);

        Debug.WriteLine($"Render took {sw.Elapsed}");

        var frozenImage = new Image
        {
            Source = rtb,
            Effect = new BlurEffect { Radius = 24 },

            // Transitions = [new EffectTransition { Duration = TimeSpan.FromMilliseconds(200), Property = EffectProperty }]
        };

        var parent = (Panel)Root.Parent!;

        Root.IsVisible = false;
        parent.Children.Add(frozenImage);

        Debug.WriteLine($"Setting content took {sw.Elapsed}");
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}
