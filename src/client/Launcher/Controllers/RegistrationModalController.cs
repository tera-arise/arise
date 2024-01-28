namespace Arise.Client.Launcher.Controllers;

internal sealed partial class RegistrationModalController : ModalController
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public RegistrationModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
    }

    [RelayCommand]
    private void Confirm()
    {
        MainController.CurrentModalController = null;
    }

    [RelayCommand]
    private void GoBack()
    {
        MainController.CurrentModalController = new LoginModalController(Services, MainController);
    }
}
