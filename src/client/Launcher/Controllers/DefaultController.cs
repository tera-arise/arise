using Arise.Client.Launcher.Controllers.Modals;
using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class DefaultController : ViewController
{
    private readonly UserSession _session;

    public override MaterialIconKind IconKind => MaterialIconKind.Home;

    public DefaultController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;
    }

    [RelayCommand]
    private Task PlayAsync()
    {
        if (!_session.IsLoggedIn)
        {
            ModalController.NavigateTo<LoginModalController>();
        }
        else if (!_session.IsVerified)
        {
            ModalController.NavigateTo<AccountVerificationModalController>();
        }
        else
        {
            return MainController.LaunchGameAsync(); // todo: use message?
        }

        return Task.CompletedTask;
    }
}
