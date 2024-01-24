namespace Arise.Client.Launcher.Controllers;

public abstract class ViewController : ObservableValidator
{
    public IServiceProvider Services { get; }

    protected ViewController(IServiceProvider services)
    {
        Services = services;
    }
}
