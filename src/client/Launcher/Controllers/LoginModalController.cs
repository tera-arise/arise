namespace Arise.Client.Launcher.Controllers;

internal sealed partial class LoginModalController : ModalController
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    private bool CanExecuteLogin => !string.IsNullOrEmpty(Email)
                                 && !string.IsNullOrEmpty(Password);

    public LoginModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
    }

    [RelayCommand(CanExecute = nameof(CanExecuteLogin))]
    private async Task LoginAsync()
    {
        // todo: add an IsLoggingIn state to signal the user that the request is being processed
        // maybe bind it to a spinning indicator

        // todo: use RememberMe setting (save it locally?)

        if (!MainController.IsLoggedIn)
        {
            // todo: catch something?
            try
            {
#if FORCE_LOGIN // find a better way of doing this when you don't have a backend
                if (await Task.FromResult(true).ConfigureAwait(true))
#else
                var resp = await MainController.Gateway.Rest
                    .AuthenticateAccountAsync(Email, Password).ConfigureAwait(true);

                if (resp.SessionTicket != null)
#endif
                {
                    MainController.IsLoggedIn = true; // todo
                    MainController.CurrentAccountName = Email; // todo

                    // todo: handle all the data in the response (ban, mail verification, etc)
                }
                else
                {
                    // todo: handle login fail
                }
            }
            catch (GatewayHttpException)
            {
                // todo
            }
            finally
            {
                // clear the password as soon as it's been sent
                Password = string.Empty;

                // IsModalVisible = false; // todo
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
