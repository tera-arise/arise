using static Windows.Win32.WindowsPInvoke;

namespace Arise.Client.Launcher;

public static class LauncherProgram
{
    public static async Task<int> RunAsync(ReadOnlyMemory<string> args)
    {
        // We build for the Windows GUI subsystem, so no console output will appear. This is not very helpful. Try
        // attaching to the parent process's console if it exists.
        _ = AttachConsole(ATTACH_PARENT_PROCESS);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        await new HostBuilder()
            .UseSerilog(static (ctx, services, cfg) =>
                cfg
                    .MinimumLevel.Is(Serilog.Events.LogEventLevel.Verbose)
                    .MinimumLevel.Override("Avalonia", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate:
                            "[{Timestamp:HH:mm:ss}][{Level:w3}][{SourceContext}] {Message:lj}{NewLine}{Exception}",
                        formatProvider: CultureInfo.InvariantCulture,
                        standardErrorFromLevel: Serilog.Events.LogEventLevel.Warning,
                        theme: AnsiConsoleTheme.Code)
                    .ReadFrom.Services(services))
            .ConfigureServices((ctx, services) =>
                services
                    .AddAriseClientLauncher()
                    .AddSingleton(
                        provider => ActivatorUtilities.CreateInstance<LauncherApplicationHost>(provider, args))
                    .AddHostedService(static provider => provider.GetRequiredService<LauncherApplicationHost>()))
            .UseDefaultServiceProvider(static opts =>
            {
                opts.ValidateOnBuild = true;
                opts.ValidateScopes = true;
            })
            .RunConsoleAsync();

        return 0;
    }
}
