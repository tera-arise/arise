namespace Arise.Client.Launcher;

internal sealed class LauncherOptions : IOptions<LauncherOptions>
{
    public bool GameConsole { get; set; }

    LauncherOptions IOptions<LauncherOptions>.Value => this;

    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        _ = services
            .AddOptions<LauncherOptions>()
            .BindConfiguration("Launcher");
    }
}
