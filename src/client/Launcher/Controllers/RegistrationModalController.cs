namespace Arise.Client.Launcher.Controllers;

internal sealed partial class RegistrationModalController : ModalController
{
    private readonly UserSession _session;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public RegistrationModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;
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
            ActionStatus = ActionStatus.Failed;
            return;
        }

        try
        {
            var resp = await MainController.Gateway.Rest
                .AuthenticateAccountAsync(Email, Password)
                .ConfigureAwait(true);

            if (resp.SessionTicket != null)
            {
                _session.Login(Email, resp, Password);

                ActionStatus = ActionStatus.Successful;

                await Task.Delay(1000).ConfigureAwait(true); // wait a bit to show feedback before closing modal

                MainController.CurrentModalController = null;
            }
            else
            {
                ActionStatus = ActionStatus.Failed;
            }
        }
        catch (GatewayHttpException)
        {
            // todo
            ActionStatus = ActionStatus.Failed;
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
