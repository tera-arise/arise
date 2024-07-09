using CommunityToolkit.Mvvm.Messaging;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class RecoverPasswordModalController : ModalController
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string _email = string.Empty;

    public bool CanConfirm => !string.IsNullOrEmpty(Email)
                              && ActionStatus is not ActionStatus.Pending;

    public RecoverPasswordModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
    }

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;
            await MainController.Gateway.Rest.Accounts
                .RecoverPasswordAsync(new AccountsRecoverPasswordRequest
                {
                    Email = Email,
                }).ConfigureAwait(true);

            ActionStatus = ActionStatus.Successful;

            await Task.Delay(1000).ConfigureAwait(true); // wait a bit to show feedback before closing modal

            GoBack();
        }
        catch (GatewayHttpException)
        {
            ActionStatus = ActionStatus.Failed;
        }
    }

    [RelayCommand]
    private static void GoBack()
    {
        NavigateTo<LoginModalController>();
    }
}
