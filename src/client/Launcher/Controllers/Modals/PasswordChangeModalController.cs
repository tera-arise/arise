namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed partial class PasswordChangeModalController : ModalController
{
    private readonly UserSession _session;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string _newPasswordRepeat = string.Empty;

    private bool CanConfirm => !string.IsNullOrEmpty(Password)
                               && !string.IsNullOrEmpty(NewPassword)
                               && Password != NewPassword
                               && NewPassword == NewPasswordRepeat
                               && ActionStatus is not ActionStatus.Pending;

    public PasswordChangeModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;
    }

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    public async Task ConfirmAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;

            await MainController.Gateway.Rest.Accounts
                .ChangePasswordAsync(
                    _session.AccountName!,
                    Password,
                    new AccountsChangePasswordRequest
                    {
                        Password = NewPassword,
                    })
                .ConfigureAwait(true);

            CloseModal();

            ActionStatus = ActionStatus.None;
        }
        catch (GatewayHttpException)
        {
            ActionStatus = ActionStatus.Failed;
        }
    }
}
