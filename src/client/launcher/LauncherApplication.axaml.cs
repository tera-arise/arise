using Arise.Client.Launcher.Controllers;
using Arise.Client.Launcher.Templates;
using Arise.Client.Launcher.Windows;

namespace Arise.Client.Launcher;

internal sealed partial class LauncherApplication : Application
{
    private readonly IServiceProvider _services;

    public LauncherApplication(IServiceProvider services)
    {
        _services = services;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        DataTemplates.Add(new WindowLocatorDataTemplate(_services));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Unsafe.As<IClassicDesktopStyleApplicationLifetime>(ApplicationLifetime!).MainWindow = new MainWindow
        {
            DataContext = new MainController(_services),
        };

        base.OnFrameworkInitializationCompleted();
    }
}
