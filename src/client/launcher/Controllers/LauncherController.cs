namespace Arise.Client.Launcher.Controllers;

internal abstract class LauncherController : ReactiveObject, IActivatableViewModel
{
    ViewModelActivator IActivatableViewModel.Activator { get; } = new();

    public IServiceProvider Services { get; }

    public string Title { get; } = ThisAssembly.GameTitle;

    protected LauncherController(IServiceProvider services)
    {
        Services = services;
    }
}
