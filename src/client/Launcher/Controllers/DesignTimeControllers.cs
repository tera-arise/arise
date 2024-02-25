using Arise.Client.Gateway;
using Arise.Client.Launcher.Media;
using Arise.Client.Launcher.Settings;

namespace Arise.Client.Launcher.Controllers;

internal static class DesignTimeControllers
{
    public static MainController Main { get; }

    public static AccountManagementController AccountManagement { get; }

    static DesignTimeControllers()
    {
        var services = new ServiceCollection()
            .AddSingleton<LauncherApplicationHost>()
            .AddSingleton<UserSession>()
            .AddHttpClient<GatewayClient>()
            .Services
            .BuildServiceProvider();

        Main = new(services, new MusicPlayer(), new LauncherSettingsManager());
        Main.Controllers.Insert(0, new AccountManagementController(services, Main));

        AccountManagement = new AccountManagementController(services, Main);
    }
}
