using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

public sealed partial class DefaultController : ViewController
{
    public override MaterialIconKind IconKind => MaterialIconKind.Home;

    public DefaultController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
    }

    [RelayCommand]
    private void Play()
    {
        if (!MainController.IsLoggedIn)
        {
            MainController.ShowLoginForm();
        }
        else
        {
            MainController.LaunchGame();
        }
    }
}
