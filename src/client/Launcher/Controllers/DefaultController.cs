using CommunityToolkit.Mvvm.Messaging;
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
            _ = WeakReferenceMessenger.Default.Send(new NavigateModalMessage(typeof(LoginModalController)));
        }
        else if (!_session.IsVerified)
        {
            _ = WeakReferenceMessenger.Default.Send(new NavigateModalMessage(typeof(AccountVerificationModalController)));
        }
        else
        {
            return MainController.LaunchGameAsync();
        }

        return Task.CompletedTask;
    }
}
