namespace Arise.Client.Launcher.Controllers;

internal sealed partial class AccountVerificationModalController : ModalController
{
    private readonly UserSession _session;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string _token = string.Empty;

    [ObservableProperty]
    private ActionStatus _actionStatus;

    public bool CanConfirm => !string.IsNullOrEmpty(Token)
                           && ActionStatus is not ActionStatus.Pending;

    public AccountVerificationModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;
    }

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;
            await MainController.Gateway.Rest.Accounts
                .VerifyAsync(_session.AccountName!, _session.Password!, new AccountsVerifyRequest
            {
                Token = Token,
            }).ConfigureAwait(true);

            _session.Verify();
            ActionStatus = ActionStatus.Successful;

            await Task.Delay(1000).ConfigureAwait(true); // wait a bit to show feedback before closing modal

            MainController.CurrentModalController = null;
        }
        catch (GatewayHttpException)
        {
            // todo: something else?
            ActionStatus = ActionStatus.Failed;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        MainController.CurrentModalController = null;
    }

    [RelayCommand]
    private async Task ResendEmailAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;
            await MainController.Gateway.Rest.Accounts
                .SendVerificationAsync(_session.AccountName!, _session.Password!)
                .ConfigureAwait(true);

            ActionStatus = ActionStatus.Successful;
        }
        catch (GatewayHttpException)
        {
            // todo: something else?
            ActionStatus = ActionStatus.Failed;
        }
    }
}
