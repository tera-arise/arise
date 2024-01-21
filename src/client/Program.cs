using Arise.Client.Game;
using Arise.Client.Game.Data;
using Arise.Client.Game.Launcher;
using Arise.Client.Game.Net;
using Arise.Client.Game.Net.Sessions;
using Arise.Client.Gateway;
using Arise.Client.Launcher;
using Arise.Client.Launcher.Controllers;
using Arise.Client.Launcher.Media;
using Arise.Client.Launcher.Windows;
using Arise.Client.Logging;
using static Windows.Win32.WindowsPInvoke;

namespace Arise.Client;

internal static class Program
{
    private static Task Main(string[] args)
    {
        var context = InjectedProgramContext.Instance;
        var isLauncher = context.InjectorProcessId == null;

        // We build for the Windows GUI subsystem, so no console output will appear in the launcher. This is not very
        // helpful. Try attaching to the parent process's console if it exists.
        if (isLauncher)
            _ = AttachConsole(ATTACH_PARENT_PROCESS);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        return new HostBuilder()
            .ConfigureAppConfiguration(builder => builder.AddCommandLine(args))
            .UseDefaultServiceProvider(static opts =>
            {
                opts.ValidateOnBuild = true;
                opts.ValidateScopes = true;
            })
            .ConfigureServices(services =>
            {
                if (!isLauncher)
                {
                    services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

                    _ = services
                        .AddOptions<GameOptions>()
                        .BindConfiguration("Game")
                        .Services
                        .AddSingleton<CodeManager, PageCodeManager>()
                        .AddSingleton<TeraLauncherProxyManager>()
                        .AddSingleton<TeraConnectionManager>()
                        .AddSingleton<GameClient>()
                        .AddSingleton<GameClientSessionDispatcher>()
                        .AddSingleton<TeraDataCenterLoader>()
                        .AddSingleton(services =>
                            ActivatorUtilities.CreateInstance<GameApplicationHost>(services, (Action)context.WakeUp))
                        .AddHostedService(static provider => provider.GetRequiredService<TeraLauncherProxyManager>())
                        .AddHostedService(static provider => provider.GetRequiredService<TeraConnectionManager>())
                        .AddHostedService(static provider => provider.GetRequiredService<GameClient>())
                        .AddHostedService(static provider => provider.GetRequiredService<TeraDataCenterLoader>())
                        .AddHostedService(static provider => provider.GetRequiredService<GameApplicationHost>());
                }
                else
                    _ = services
                        .AddOptions<LauncherOptions>()
                        .BindConfiguration("Launcher")
                        .Services
                        .AddTransient<MainController>()
                        .AddTransient<MainWindow>()
                        .AddSingleton<MusicPlayer>()
                        .AddSingleton<AvaloniaLogSink>()
                        .AddSingleton<LauncherApplicationHost>()
                        .AddHttpClient<GatewayClient>()
                        .Services
                        .AddHostedService(static provider => provider.GetRequiredService<LauncherApplicationHost>());
            })
            .UseSerilog((ctx, services, cfg) =>
            {
                // Make game logging output available in a new console window if requested.
                if (!isLauncher && services.GetRequiredService<IOptions<GameOptions>>().Value.Console)
                    _ = AllocConsole();

                _ = cfg
                    .MinimumLevel.Is(Serilog.Events.LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate:
                            "[{Timestamp:HH:mm:ss}][{Level:w3}][{SourceContext}] {Message:lj}{NewLine}{Exception}",
                        formatProvider: CultureInfo.InvariantCulture,
                        standardErrorFromLevel: Serilog.Events.LogEventLevel.Warning,
                        theme: AnsiConsoleTheme.Code)
                    .ReadFrom.Services(services);
            })
            .RunConsoleAsync();
    }

    // Required by the Avalonia designer. Must have this exact signature.
    public static AppBuilder BuildAvaloniaApp()
    {
        return BuildAvaloniaApp(static () => new());
    }

    public static AppBuilder BuildAvaloniaApp(Func<LauncherApplication> factory)
    {
        return AppBuilder
            .Configure(factory)
            .UseWin32()
            .UseSkia()
            .WithInterFont();
    }
}
