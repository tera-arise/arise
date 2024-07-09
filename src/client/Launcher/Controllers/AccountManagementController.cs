using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

public class AccountManagementController : ViewController
{
    public override MaterialIconKind IconKind => MaterialIconKind.Account;

    public AccountManagementController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
    }
}
