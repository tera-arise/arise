using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

public abstract partial class ViewController : ObservableValidator
{
    public IServiceProvider Services { get; }

    private readonly MainController _mainController;

    public abstract MaterialIconKind IconKind { get; }

    protected ViewController(IServiceProvider services, MainController mainController)
    {
        Services = services;
        _mainController = mainController;
    }
}
