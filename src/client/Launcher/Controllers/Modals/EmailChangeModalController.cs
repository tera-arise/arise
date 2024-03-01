namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed partial class EmailChangeModalController : ModalController
{
    private readonly UserSession _session;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestChangeCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestChangeCommand))]
    private string _newEmail = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyChangeCommand))]
    private string _token = string.Empty;

    [ObservableProperty]
    private bool _isChangeInProgress;

    [ObservableProperty]
    private bool _isPasswordRequired = true;

    [ObservableProperty]
    private ActionStatus _sendEmailActionStatus;

    private bool CanRequest => !string.IsNullOrEmpty(Password)
                            && !string.IsNullOrEmpty(NewEmail)
                            && _session.AccountName != NewEmail
                            && ActionStatus is not ActionStatus.Pending;

    private bool CanVerify => !string.IsNullOrEmpty(Token)
                            && ActionStatus is not ActionStatus.Pending;

    public EmailChangeModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;

        if (_session.IsChangingEmail)
        {
            IsChangeInProgress = true;
        }

        if (_session.Password != null)
        {
            IsPasswordRequired = false;
            Password = _session.Password!;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRequest))]
    private async Task RequestChangeAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;

            await SendChangeRequestAsync()
                .ConfigureAwait(true);

            IsChangeInProgress = true;
            ActionStatus = ActionStatus.None;

            _session.BeginEmailChange();
        }
        catch (GatewayHttpException)
        {
            ActionStatus = ActionStatus.Failed;
        }
    }

    [RelayCommand]
    private async Task ResendEmailAsync()
    {
        try
        {
            SendEmailActionStatus = ActionStatus.Pending;

            await SendChangeRequestAsync()
                .ConfigureAwait(true);

            SendEmailActionStatus = ActionStatus.None;
        }
        catch (GatewayHttpException)
        {
            SendEmailActionStatus = ActionStatus.Failed;
        }
    }

    private async Task SendChangeRequestAsync()
    {
        await MainController.Gateway.Rest.Accounts
            .ChangeEmailAsync(
                _session.AccountName!,
                Password,
                new AccountsChangeEmailRequest
                {
                    Email = NewEmail,
                })
            .ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(CanVerify))]
    private async Task VerifyChangeAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;

            var resp = await MainController.Gateway.Rest.Accounts
                .VerifyEmailChangeAsync(
                    _session.AccountName!,
                    Password,
                    new AccountsVerifyEmailChangeRequest
                    {
                        Token = Token,
                    })
                .ConfigureAwait(true);

            ActionStatus = ActionStatus.Successful;

            await Task.Delay(1000).ConfigureAwait(true);

            _session.VerifyEmailChange(resp.Email);

            CloseModal();
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
}
