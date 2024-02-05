using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal abstract partial class ViewController : ObservableValidator
{
    protected MainController MainController { get; }

    public IServiceProvider Services { get; }

    public abstract MaterialIconKind IconKind { get; }

    protected ViewController(IServiceProvider services, MainController mainController)
    {
        MainController = mainController;
        Services = services;
    }
}
