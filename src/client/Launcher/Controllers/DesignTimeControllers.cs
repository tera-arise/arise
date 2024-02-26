using Arise.Client.Gateway;
using Arise.Client.Launcher.Controllers.Modals;
using Arise.Client.Launcher.Media;
using Arise.Client.Launcher.Settings;

namespace Arise.Client.Launcher.Controllers;

internal static class DesignTimeControllers
{
    public static MainController Main { get; }

    public static AccountManagementController AccountManagement { get; }

    public static EmailChangeModalController EmailChange { get; }

    public static PasswordChangeModalController PasswordChange { get; }

    static DesignTimeControllers()
    {
        var services = new ServiceCollection()
            .AddSingleton<LauncherApplicationHost>()
            .AddSingleton<UserSession>()
            .AddSingleton<MusicPlayer>()
            .AddSingleton<LauncherSettingsManager>()
            .AddHttpClient<GatewayClient>()
            .Services
            .BuildServiceProvider();

        Main = new(services);
        Main.Controllers.Insert(0, new AccountManagementController(services, Main));

        AccountManagement = new AccountManagementController(services, Main);

        var usersession = services.GetService<UserSession>()!;
        usersession.Login(
            "test",
            new AccountsAuthenticateResponse
            {
                BanReason = null,
                IsChangingEmail = false,
                IsDeleting = false,
                IsRecovered = false,
                IsVerifying = false,
                SessionTicket = null,
            },
            "123");

        EmailChange = new EmailChangeModalController(services, Main);
        PasswordChange = new PasswordChangeModalController(services, Main);
    }
}
