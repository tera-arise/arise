using Arise.Client.Data;
using Arise.Client.Net;
using static Windows.Win32.WindowsPInvoke;

namespace Arise.Client;

public static class SymbioteProgram
{
    public static async Task<int> RunAsync(ReadOnlyMemory<string> args, Action wake)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        await new HostBuilder()
            .ConfigureAppConfiguration(builder => builder.AddCommandLine(args.ToArray()))
            .ConfigureServices(services =>
            {
                services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

                _ = services
                    .AddOptions<SymbioteOptions>()
                    .BindConfiguration("Symbiote")
                    .Services
                    .AddSingleton<CodeManager, PageCodeManager>()
                    .AddSingleton(services => ActivatorUtilities.CreateInstance<GameApplicationHost>(services, wake))
                    .AddAriseClientSymbiote()
                    .AddHostedService(static provider => provider.GetRequiredService<TeraConnectionManager>())
                    .AddHostedService(static provider => provider.GetRequiredService<GameClient>())
                    .AddHostedService(static provider => provider.GetRequiredService<DataCenterLoader>())
                    .AddHostedService(static provider => provider.GetRequiredService<GameApplicationHost>());
            })
            .UseDefaultServiceProvider(static opts =>
            {
                opts.ValidateOnBuild = true;
                opts.ValidateScopes = true;
            })
            .UseSerilog(static (ctx, services, cfg) =>
            {
                if (services.GetRequiredService<IOptions<SymbioteOptions>>().Value.Console)
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
            .RunConsoleAsync();

        return 0;
    }
}
