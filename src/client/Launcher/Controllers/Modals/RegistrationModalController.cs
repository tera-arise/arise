// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed partial class RegistrationModalController : ModalController
{
    private readonly UserSession _session;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public RegistrationModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        try
        {
            ActionStatus = ActionStatus.Pending;
            var request = new AccountsCreateRequest
            {
                Email = Email,
                Password = Password,
            };

            await MainController.Gateway.Rest.Accounts
                .CreateAsync(request)
                .ConfigureAwait(true);
        }
        catch (GatewayHttpException)
        {
            Password = string.Empty;
            ActionStatus = ActionStatus.Failed;
            return;
        }

        try
        {
            var resp = await MainController.Gateway.Rest.Accounts
                .AuthenticateAsync(Email, Password)
                .ConfigureAwait(true);

            if (resp.SessionTicket != null)
            {
                _session.Login(Email, resp, Password);

                ActionStatus = ActionStatus.Successful;

                await Task.Delay(1000).ConfigureAwait(true); // wait a bit to show feedback before closing modal

                CloseModal();
            }
            else
            {
                ActionStatus = ActionStatus.Failed;
            }
        }
        catch (GatewayHttpException)
        {
            ActionStatus = ActionStatus.Failed;
        }
        finally
        {
            Password = string.Empty;
        }
    }

    [RelayCommand]
    private static void GoBack()
    {
        NavigateTo<LoginModalController>();
    }
}
