namespace Arise.Client.Launcher.Controllers;

public abstract class LauncherController : ObservableObject
{
    public IServiceProvider Services { get; }

    public string Title { get; } = ThisAssembly.GameTitle;

    protected LauncherController(IServiceProvider services)
    {
        Services = services;
    }
}
