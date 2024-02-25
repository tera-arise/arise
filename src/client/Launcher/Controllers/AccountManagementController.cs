using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class AccountManagementController : ViewController
{
    public override MaterialIconKind IconKind => MaterialIconKind.Account;

    public AccountManagementController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
    }

    [RelayCommand]
    [SuppressMessage("", "CA1822")]
    private void ChangeEmail()
    {
        // only if logged in
        // only if verified
    }

    [RelayCommand]
    [SuppressMessage("", "CA1822")]
    private void ChangePassword()
    {
        // only if logged in
    }

    [RelayCommand]
    [SuppressMessage("", "CA1822")]
    private void DeleteAccount()
    {
        // only if logged in
    }
}
