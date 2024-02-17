using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class DefaultController : ViewController
{
    private readonly UserSession _session;

    public override MaterialIconKind IconKind => MaterialIconKind.Home;

    public DefaultController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;
    }

    [RelayCommand]
    private void Play()
    {
        if (!_session.IsLoggedIn)
        {
            MainController.ShowLoginForm();
        }
        else
        {
            MainController.LaunchGame();
        }
    }
}
