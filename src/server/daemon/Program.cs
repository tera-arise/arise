using Arise.Server.Daemon;

internal static class Program
{
    [SuppressMessage("", "CA1308")]
    private static Task<int> Main(string[] args)
    {
        TaskScheduler.UnobservedTaskException += static (_, e) =>
        {
            // TODO: https://github.com/dotnet/runtime/issues/80111
            if (!e.Exception.InnerExceptions.Any(static ex => ex is QuicException))
                ExceptionDispatchInfo.Throw(e.Exception);
        };

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        using var parser = new Parser(static settings =>
        {
            settings.GetoptMode = true;
            settings.PosixlyCorrect = true;
            settings.CaseSensitive = false;
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = Console.Error;
        });

        return parser
            .ParseArguments<DaemonOptions>(args)
            .MapResult(
                static async options =>
                {
                    var flags = options.Services;

                    var builder = new HostBuilder()
                        .UseEnvironment(options.Environment.ToString())
                        .ConfigureAppConfiguration(static (ctx, builder) =>
                            builder
                                .AddJsonFile($"{ThisAssembly.AssemblyName}.json")
                                .AddJsonFile(
                                    $"{ThisAssembly.AssemblyName}." +
                                    $"{ctx.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json"))
                        .ConfigureServices(services =>
                        {
                            _ = services.AddStorageServices();

                            if (flags.HasFlag(DaemonServices.Web))
                                _ = services.AddWebServices();

                            if (flags.HasFlag(DaemonServices.World))
                                _ = services.AddWorldServices();
                        })
                        .UseDefaultServiceProvider(static opts =>
                        {
                            opts.ValidateOnBuild = true;
                            opts.ValidateScopes = true;
                        })
                        .UseSerilog(static (ctx, services, cfg) =>
                            cfg
                                .MinimumLevel.Is(LogEventLevel.Information)
                                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                                .Enrich.FromLogContext()
                                .ReadFrom.Configuration(ctx.Configuration)
                                .ReadFrom.Services(services));

                    if (flags.HasFlag(DaemonServices.Web))
                        _ = builder.ConfigureWebServices(static builder => builder.UseSerilogRequestLogging());

                    await builder
                        .ConfigureHostOptions(static opts => opts.ShutdownTimeout = TimeSpan.FromMinutes(1))
                        .UseSystemd()
                        .RunConsoleAsync();

                    return 0;
                },
                static _ => Task.FromResult(1));
    }
}
