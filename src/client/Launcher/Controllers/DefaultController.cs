using System.Windows.Input;

namespace Arise.Client.Launcher.Controllers;

public sealed class DefaultController : ViewController
{
    private readonly MainController _mainController;

    public ICommand PlayCommand { get; }

    public DefaultController(IServiceProvider services, MainController mainController)
        : base(services)
    {
        _mainController = mainController;
        PlayCommand = new RelayCommand(Play);
    }

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
