namespace Arise.Client.Launcher.Controllers;

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
    private void CloseModal()
    {
        // IsModalVisible = false; // todo: checks
        MainController.CurrentModalController = null;
    }
}
