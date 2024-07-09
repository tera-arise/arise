using Arise.Client.Launcher.Controllers.Modals;
using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class AccountManagementController : ViewController
{
    public override MaterialIconKind IconKind => MaterialIconKind.Account;

    private bool CanChangeEmail => MainController.IsVerified;

    private bool CanChangePassword => MainController.IsLoggedIn;

    private bool CanDeleteAccount => MainController.IsLoggedIn;

    public AccountManagementController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
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
}
