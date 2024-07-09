namespace Arise.Client.Launcher.Controllers;

public partial class ModalController : LauncherController
{
    protected MainController MainController { get; set; }

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
