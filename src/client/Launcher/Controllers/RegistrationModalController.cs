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
            var result = await MainController.Gateway.Rest
                .CreateAccountAsync(new AccountsCreateRequest
                {
                    Email = Email,
                    Password = Password,
                }).ConfigureAwait(true);

            if (!string.IsNullOrEmpty(result.SessionTicket))
            {
                MainController.IsLoggedIn = true;
            }
        }
        catch (GatewayHttpException)
        {
            // todo
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        MainController.CurrentModalController = new LoginModalController(Services, MainController);
    }
}
