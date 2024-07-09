namespace Arise.Client.Launcher.Controllers;

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
    private bool _rememberMe;

    [ObservableProperty]
    private LoginState _loginState;

    private bool CanExecuteLogin => !string.IsNullOrEmpty(Email)
                                    && !string.IsNullOrEmpty(Password);

    public LoginModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteLogin))]
    private async Task LoginAsync()
    {
        // todo: use RememberMe setting (save it locally?)

        if (!_session.IsLoggedIn)
        {
            try
            {
                LoginState = LoginState.Pending;

                var resp = await MainController.Gateway.Rest
                    .AuthenticateAccountAsync(Email, Password).ConfigureAwait(true);

                if (resp.SessionTicket != null)
                {
                    _session.Login(Email, resp);

                    LoginState = LoginState.Successful;

                    MainController.CurrentModalController = null;
                }
                else
                {
                    LoginState = LoginState.Failed;
                }
            }
            catch (GatewayHttpException)
            {
                // todo
                LoginState = LoginState.Failed;
            }
            finally
            {
                Password = string.Empty;
            }
        }
        else
        {
            // todo: warn?
        }
    }

    [RelayCommand]
    [SuppressMessage("", "CA1822")]
    private void RecoverPassword()
    {
        // todo
    }

    [RelayCommand]
    private void Register()
    {
        MainController.CurrentModalController = new RegistrationModalController(Services, MainController);
    }
}
