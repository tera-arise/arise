// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Daemon;

public static class DaemonHost
{
    [SuppressMessage("", "CA1308")]
    public static Task<int> RunAsync(
        IEnumerable<string> args,
        IEnumerable<string> names,
        Action<IServiceCollection> configureServices,
        Action<LoggerConfiguration>? configureLogger = null,
        Action<IHostBuilder>? configureHost = null)
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
                async options =>
                {
                    var builder = new HostBuilder()
                        .UseEnvironment(options.Environment.ToString())
                        .ConfigureAppConfiguration((ctx, builder) =>
                        {
                            var env = ctx.HostingEnvironment.EnvironmentName.ToLowerInvariant();

                            _ = builder
                                .AddJsonFile("arised.json", optional: true)
                                .AddJsonFile($"arised.{env}.json", optional: true);

                            foreach (var name in names)
                                _ = builder
                                    .AddJsonFile($"arise-{name}d.json", optional: true)
                                    .AddJsonFile($"arise-{name}d.{env}.json", optional: true);
                        })
                        .ConfigureServices(services => configureServices(services.AddStorageServices()))
                        .UseDefaultServiceProvider(static opts =>
                        {
                            opts.ValidateOnBuild = true;
                            opts.ValidateScopes = true;
                        })
                        .UseSerilog((ctx, services, cfg) =>
                        {
                            configureLogger?.Invoke(cfg);

                            _ = cfg
                                .MinimumLevel.Is(LogEventLevel.Information)
                                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                                .Enrich.FromLogContext()
                                .ReadFrom.Configuration(
                                    ctx.Configuration,
                                    new ConfigurationReaderOptions
                                    {
                                        AllowInternalTypes = true,
                                        AllowInternalMethods = true,
                                    })
                                .ReadFrom.Services(services);
                        });

                    configureHost?.Invoke(builder);

                    await builder
                        .ConfigureHostOptions(static opts => opts.ShutdownTimeout = TimeSpan.FromMinutes(1))
                        .UseSystemd()
                        .RunConsoleAsync();

                    return 0;
                },
                static _ => Task.FromResult(1));
    }
}
