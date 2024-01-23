namespace Arise.Client.Launcher.Controllers;

public abstract class ViewController : ObservableObject
{
    public IServiceProvider Services { get; }

    protected ViewController(IServiceProvider services)
    {
        Services = services;
    }
}
