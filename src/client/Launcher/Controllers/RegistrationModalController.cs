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
    private async Task ConfirmAsync()
    {
        MainController.CurrentModalController = null;

        try
        {
            var request = new AccountsCreateRequest
            {
                Email = Email,
                Password = Password,
            };

            await MainController.Gateway.Rest
                .CreateAccountAsync(request)
                .ConfigureAwait(true);
        }
        catch (GatewayHttpException)
        {
            // todo
            Password = string.Empty;
            return;
        }

        try
        {
            var loginResult = await MainController.Gateway.Rest
                .AuthenticateAccountAsync(Email, Password)
                .ConfigureAwait(true);

            if (!string.IsNullOrEmpty(loginResult.SessionTicket))
            {
                MainController.IsLoggedIn = true;
            }
        }
        catch (GatewayHttpException)
        {
            // todo
        }
        finally
        {
            Password = string.Empty;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        MainController.CurrentModalController = new LoginModalController(Services, MainController);
    }
}
