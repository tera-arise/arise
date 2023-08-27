namespace Arise.Client.Launcher.Controllers;

internal abstract class LauncherController : ReactiveObject, IActivatableViewModel
{
    ViewModelActivator IActivatableViewModel.Activator { get; } = new();

    public IServiceProvider Services { get; }

    protected LauncherController(IServiceProvider services)
    {
        Services = services;
    }
}
