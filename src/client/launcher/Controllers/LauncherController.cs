namespace Arise.Client.Launcher.Controllers;

internal abstract class LauncherController : ReactiveObject
{
    public IServiceProvider Services { get; }

    protected LauncherController(IServiceProvider services)
    {
        Services = services;
    }
}
