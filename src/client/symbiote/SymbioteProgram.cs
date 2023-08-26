using Arise.Client.Net;
using static Windows.Win32.WindowsPInvoke;

namespace Arise.Client;

public static class SymbioteProgram
{
    public static async Task<int> RunAsync(ReadOnlyMemory<string> args, Action waker)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        await new HostBuilder()
            .ConfigureHostConfiguration(builder => builder.AddCommandLine(args.ToArray()))
            .ConfigureAppConfiguration(builder => builder.AddCommandLine(args.ToArray()))
            .UseSerilog(static (ctx, services, cfg) =>
            {
                if (ctx.Configuration["Console"] is { } str && bool.Parse(str))
                    _ = AllocConsole();

                _ = cfg
                    .MinimumLevel.Is(LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate:
                            "[{Timestamp:HH:mm:ss}][{Level:w3}][{SourceContext}] {Message:lj}{NewLine}{Exception}",
                        formatProvider: CultureInfo.InvariantCulture,
                        standardErrorFromLevel: LogEventLevel.Warning,
                        theme: AnsiConsoleTheme.Code)
                    .ReadFrom.Services(services);
            })
            .ConfigureServices(services =>
            {
                services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

                _ = services
                    .AddOptions<SymbioteOptions>()
                    .BindConfiguration("Symbiote")
                    .Services
                    .AddSingleton(services => ActivatorUtilities.CreateInstance<GameApplicationHost>(services, waker))
                    .AddAriseClientSymbiote()
                    .AddHostedService(static provider => provider.GetRequiredService<GameClient>())
                    .AddHostedService(static provider => provider.GetRequiredService<GameApplicationHost>());
            })
            .UseDefaultServiceProvider(static opts =>
            {
                opts.ValidateOnBuild = true;
                opts.ValidateScopes = true;
            })
            .RunConsoleAsync();

        return 0;
    }
}
