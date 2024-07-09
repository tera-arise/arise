// SPDX-License-Identifier: AGPL-3.0-or-later
using Arise.Client.Launcher.Controllers.Modals;
using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class AccountManagementController : ViewController
{
    private readonly UserSession _session;

    public override MaterialIconKind IconKind => MaterialIconKind.Account;

    private bool CanChangeEmail => _session is { IsVerified: true, IsLoggedIn: true, DeletionDue: null };

    private bool CanChangePassword => _session is { IsLoggedIn: true, DeletionDue: null };

    private bool CanDeleteAccount => _session is { IsLoggedIn: true, DeletionDue: null };

    private bool CanCancelAccountDeletion => IsDeletionPending;

    public bool IsDeletionPending => _session is { IsLoggedIn: true, DeletionDue: not null };

    public DateTime? DeletionDue => _session.DeletionDue;

    public AccountManagementController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = Services.GetService<UserSession>()!;
        _session.StatusChanged += OnSessionStatusChanged;
    }

    private void OnSessionStatusChanged()
    {
        OnPropertyChanged(nameof(IsDeletionPending));
        OnPropertyChanged(nameof(DeletionDue));

        DeleteAccountCommand.NotifyCanExecuteChanged();
        CancelAccountDeletionCommand.NotifyCanExecuteChanged();
        ChangeEmailCommand.NotifyCanExecuteChanged();
        ChangePasswordCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanChangeEmail))]
    private static void ChangeEmail()
    {
        ModalController.NavigateTo<EmailChangeModalController>();
    }

    [RelayCommand(CanExecute = nameof(CanChangePassword))]
    private static void ChangePassword()
    {
        ModalController.NavigateTo<PasswordChangeModalController>();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteAccount))]
    private static void DeleteAccount()
    {
        ModalController.NavigateTo<AccountDeletionModalController>();
    }

    [RelayCommand(CanExecute = nameof(CanCancelAccountDeletion))]
    private void CancelAccountDeletion()
    {
        ModalController.NavigateTo<CancelAccountDeletionModalController>();
    }
}
