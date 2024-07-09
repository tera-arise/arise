namespace Arise.Client.Launcher.Controllers;

public sealed partial class DefaultController : ViewController
{
    private readonly MainController _mainController;

    public DefaultController(IServiceProvider services, MainController mainController)
        : base(services)
    {
        _mainController = mainController;
    }

    [RelayCommand]
    private void Play()
    {
        if (!_mainController.IsLoggedIn)
        {
            _mainController.ShowLoginForm();
        }
        else
        {
            _mainController.LaunchGame();
        }
    }
}
