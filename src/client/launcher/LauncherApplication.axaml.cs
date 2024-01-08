using Arise.Client.Launcher.Controllers;
using Arise.Client.Launcher.Templates;
using Arise.Client.Launcher.Windows;

namespace Arise.Client.Launcher;

internal sealed partial class LauncherApplication : Application
{
    private readonly IServiceProvider _services;

    private readonly IHostApplicationLifetime _hostLifetime;

    public LauncherApplication(IServiceProvider services, IHostApplicationLifetime hostLifetime)
    {
        _services = services;
        _hostLifetime = hostLifetime;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        DataTemplates.Add(new WindowLocatorDataTemplate(_services));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var lifetime = Unsafe.As<IClassicDesktopStyleApplicationLifetime>(ApplicationLifetime!);

        // Ensure that Ctrl-C in the console works as expected.
        _ = _hostLifetime.ApplicationStopping.UnsafeRegister(
            static lifetime =>
                Dispatcher.UIThread.Post(
                    () => Unsafe.As<IClassicDesktopStyleApplicationLifetime>(lifetime!).Shutdown(),
                    DispatcherPriority.MaxValue),
            state: lifetime);

        var window = ActivatorUtilities.CreateInstance<MainWindow>(_services);

        window.DataContext = ActivatorUtilities.CreateInstance<MainController>(_services);

        lifetime.MainWindow = window;

        base.OnFrameworkInitializationCompleted();
    }
}
