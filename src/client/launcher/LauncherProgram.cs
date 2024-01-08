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
            .ConfigureAppConfiguration(builder => builder.AddCommandLine(args.ToArray()))
            .UseDefaultServiceProvider(static opts =>
            {
                opts.ValidateOnBuild = true;
                opts.ValidateScopes = true;
            })
            .ConfigureServices(static services =>
                services
                    .AddAriseClientLauncher()
                    .AddHostedService(static provider => provider.GetRequiredService<LauncherApplicationHost>()))
            .UseSerilog(static (_, services, cfg) =>
                cfg
                    .MinimumLevel.Is(Serilog.Events.LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate:
                            "[{Timestamp:HH:mm:ss}][{Level:w3}][{SourceContext}] {Message:lj}{NewLine}{Exception}",
                        formatProvider: CultureInfo.InvariantCulture,
                        standardErrorFromLevel: Serilog.Events.LogEventLevel.Warning,
                        theme: AnsiConsoleTheme.Code)
                    .ReadFrom.Services(services))
            .RunConsoleAsync();

        return 0;
    }
}
