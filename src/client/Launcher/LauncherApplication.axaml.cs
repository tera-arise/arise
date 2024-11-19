// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Launcher.Controllers;
using Arise.Client.Launcher.Templates;
using Arise.Client.Launcher.Windows;

namespace Arise.Client.Launcher;

[SuppressMessage("", "CA1515")]
public sealed partial class LauncherApplication : Application
{
    private readonly IServiceProvider? _services;

    // Required by the Avalonia designer.
    public LauncherApplication()
    {
    }

    public LauncherApplication(IServiceProvider services)
    {
        _services = services;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Are we running in the Avalonia designer?
        if (_services == null)
            return;

        DataTemplates.Add(new WindowLocatorDataTemplate(_services));

        var hostLifetime = _services.GetRequiredService<IHostApplicationLifetime>();
        var avaloniaLifetime = Unsafe.As<IClassicDesktopStyleApplicationLifetime>(ApplicationLifetime!);

        // Ensure that Ctrl-C in the console works as expected.
        _ = hostLifetime.ApplicationStopping.UnsafeRegister(
            static avaloniaLifetime =>
                Dispatcher.UIThread.Post(
                    () => Unsafe.As<IClassicDesktopStyleApplicationLifetime>(avaloniaLifetime!).Shutdown(),
                    DispatcherPriority.MaxValue),
            state: avaloniaLifetime);

        var window = ActivatorUtilities.CreateInstance<MainWindow>(_services);

        window.DataContext = ActivatorUtilities.CreateInstance<MainController>(_services);

        avaloniaLifetime.MainWindow = window;

        base.OnFrameworkInitializationCompleted();
    }
}
