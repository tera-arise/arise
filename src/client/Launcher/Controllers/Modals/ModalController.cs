using CommunityToolkit.Mvvm.Messaging;

namespace Arise.Client.Launcher.Controllers.Modals;

internal partial class ModalController : LauncherController
{
    [ObservableProperty]
    private ActionStatus _actionStatus;

    protected MainController MainController { get; }

    public ModalController(IServiceProvider services, MainController mainController)
        : base(services)
    {
        MainController = mainController;
    }

    [RelayCommand]
    public static void CloseModal()
    {
        _ = WeakReferenceMessenger.Default.Send(new NavigateModalMessage(null));
    }

    public static void NavigateTo<TModal>()
        where TModal : ModalController
    {
        _ = WeakReferenceMessenger.Default.Send(new NavigateModalMessage(typeof(TModal)));
    }
}