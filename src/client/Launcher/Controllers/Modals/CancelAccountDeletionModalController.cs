// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed partial class CancelAccountDeletionModalController : ModalController
{
    private readonly UserSession _session;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestCancellationCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestCancellationCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestCancellationCommand))]
    private bool _isDeletionPending;

    private bool CanRequest => !string.IsNullOrEmpty(Password)
                               && !string.IsNullOrEmpty(Email)
                               && IsDeletionPending
                               && ActionStatus is not ActionStatus.Pending
                               ;

    public CancelAccountDeletionModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)

    {
        _session = services.GetService<UserSession>()!;

        IsDeletionPending = _session.DeletionDue != null;
    }

    [RelayCommand(CanExecute = nameof(CanRequest))]
    public async Task RequestCancellationAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;

            await MainController.Gateway.Rest.Accounts
                .CancelDeletionAsync(
                    Email,
                    Password)
                .ConfigureAwait(true);

            _session.CancelDeletion();

            CloseModal();

            ActionStatus = ActionStatus.None;
        }
        catch (GatewayHttpException)
        {
            ActionStatus = ActionStatus.Failed;
        }
    }
}
