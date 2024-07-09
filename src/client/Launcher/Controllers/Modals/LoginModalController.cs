namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed partial class LoginModalController : ModalController
{
    private readonly UserSession _session;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private ActionStatus _actionStatus;

    private bool CanExecuteLogin => !string.IsNullOrEmpty(Email)
                                 && !string.IsNullOrEmpty(Password)
                                 && ActionStatus is not ActionStatus.Pending;

    public LoginModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteLogin))]
    private async Task LoginAsync()
    {
        if (_session.IsLoggedIn)
        {
            return;
        }

        try
        {
            ActionStatus = ActionStatus.Pending;

            var resp = await MainController.Gateway.Rest.Accounts
                .AuthenticateAsync(Email, Password).ConfigureAwait(true);

            if (resp.SessionTicket != null)
            {
                _session.Login(Email, resp, resp.IsVerifying ? Password : null);

                ActionStatus = ActionStatus.Successful;

                await Task.Delay(1000).ConfigureAwait(true);

                CloseModal();
            }
            else
            {
                ActionStatus = ActionStatus.Failed;
            }
        }
        catch (GatewayHttpException)
        {
            ActionStatus = ActionStatus.Failed;
        }
        finally
        {
            Password = string.Empty;
        }
    }

    [RelayCommand]
    private static void RecoverPassword()
    {
        NavigateTo<RecoverPasswordModalController>();
    }

    [RelayCommand]
    private static void Register()
    {
        NavigateTo<RegistrationModalController>();
    }
}
