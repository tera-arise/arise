using Arise.Server.Daemon;

internal static class Program
{
    [SuppressMessage("", "CA1305")]
    [SuppressMessage("", "CA1308")]
    private static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        using var parser = new Parser(settings =>
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
                async options =>
                {
                    var flags = options.Services;
                    var builder = new HostBuilder()
                        .UseEnvironment(options.Environment.ToString())
                        .ConfigureAppConfiguration((ctx, builder) =>
                            builder
                                .AddJsonFile($"{ThisAssembly.AssemblyName}.json", optional: false, reloadOnChange: true)
                                .AddJsonFile(
                                    $"{ThisAssembly.AssemblyName}." +
                                    $"{ctx.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json",
                                    optional: false,
                                    reloadOnChange: true))
                        .UseSerilog((ctx, services, cfg) =>
                            cfg
                                .ReadFrom.Configuration(ctx.Configuration)
                                .ReadFrom.Services(services))
                        .ConfigureServices((ctx, services) =>
                        {
                            _ = services.AddStorageServices();

                            if (flags.HasFlag(DaemonServices.Web))
                                _ = services.AddWebServices();

                            if (flags.HasFlag(DaemonServices.World))
                                _ = services.AddWorldServices();
                        })
                        .UseDefaultServiceProvider(opts =>
                        {
                            opts.ValidateOnBuild = true;
                            opts.ValidateScopes = true;
                        });

                    if (flags.HasFlag(DaemonServices.Web))
                        _ = builder.ConfigureWebServices(builder => builder.UseSerilogRequestLogging());

                    await builder
                        .ConfigureHostOptions(opts => opts.ShutdownTimeout = TimeSpan.FromMinutes(1))
                        .UseSystemd()
                        .RunConsoleAsync();

                    return 0;
                },
                _ => Task.FromResult(1));
    }
}
