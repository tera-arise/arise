// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed partial class AccountDeletionModalController : ModalController
{
    private readonly UserSession _session;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestDeletionCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestDeletionCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyDeletionCommand))]
    private string _token = string.Empty;

    private bool CanRequest => !string.IsNullOrEmpty(Password)
                            && !string.IsNullOrEmpty(Email)
                            && !IsDeletionPending
                            && !IsDeletionRequested
                            && ActionStatus is not ActionStatus.Pending
                            ;

    private bool CanVerify => !string.IsNullOrEmpty(Token)
                            && IsDeletionRequested
                            && !IsDeletionPending
                            && ActionStatus is not ActionStatus.Pending
                            ;

    [ObservableProperty]
    private ActionStatus _sendEmailActionStatus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestDeletionCommand))]
    private bool _isDeletionPending;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestDeletionCommand))]
    [NotifyCanExecuteChangedFor(nameof(VerifyDeletionCommand))]
    private bool _isDeletionRequested;

    [ObservableProperty]
    private DateTime? _deletionDue;

    public AccountDeletionModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;

        DeletionDue = _session.DeletionDue;
        IsDeletionPending = DeletionDue != null;
    }

    [RelayCommand(CanExecute = nameof(CanRequest))]
    public async Task RequestDeletionAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;

            await SendDeletionRequestAsync()
                .ConfigureAwait(true);

            IsDeletionRequested = true;

            ActionStatus = ActionStatus.None;
        }
        catch (GatewayHttpException)
        {
            ActionStatus = ActionStatus.Failed;
        }
    }

    private async Task SendDeletionRequestAsync()
    {
        await MainController.Gateway.Rest.Accounts
            .DeleteAsync(
                Email,
                Password)
            .ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task ResendEmailAsync()
    {
        try
        {
            SendEmailActionStatus = ActionStatus.Pending;

            await SendDeletionRequestAsync()
                .ConfigureAwait(true);

            SendEmailActionStatus = ActionStatus.None;
        }
        catch (GatewayHttpException)
        {
            SendEmailActionStatus = ActionStatus.Failed;
        }
    }

    [RelayCommand(CanExecute = nameof(CanVerify))]
    private async Task VerifyDeletionAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;

            var resp = await MainController.Gateway.Rest.Accounts
                .VerifyDeletionAsync(
                    _session.AccountName!,
                    Password,
                    new AccountsVerifyDeletionRequest
                    {
                        Token = Token,
                    })
                .ConfigureAwait(true);

            ActionStatus = ActionStatus.Successful;

            IsDeletionPending = true;

            await Task.Delay(1000).ConfigureAwait(true);

            _session.VerifyDeletion(resp.Due);

            CloseModal(); // todo: better feedback before closing
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
