using Arise.Server.Daemon;

using var parser = new Parser(settings =>
{
    settings.GetoptMode = true;
    settings.PosixlyCorrect = true;
    settings.CaseSensitive = false;
    settings.CaseInsensitiveEnumValues = true;
    settings.HelpWriter = Terminal.StandardError.TextWriter;
});

return await parser
    .ParseArguments<DaemonOptions>(args)
    .MapResult(
        async options =>
        {
            var flags = options.Services;
            var builder = new HostBuilder()
                .UseEnvironment(options.Environment.ToString())
                .ConfigureAppConfiguration((ctx, builder) =>
                    builder.AddJsonFile(
                        $"{ThisAssembly.AssemblyName}.{ctx.HostingEnvironment.EnvironmentName}.json", false, true))
                .ConfigureLogging((ctx, builder) =>
                    builder
                        .AddConfiguration(ctx.Configuration.GetSection("Logging"))
                        .AddTerminal(opts =>
                        {
                            opts.UseUtcTimestamp = true;
                            opts.LogToStandardErrorThreshold = LogLevel.Warning;
                        }))
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
                _ = builder.ConfigureWebServices();

            await builder
                .ConfigureHostOptions(opts => opts.ShutdownTimeout = TimeSpan.FromMinutes(1))
                .UseTerminalSystemd()
                .RunConsoleAsync();

            return 0;
        },
        _ => Task.FromResult(1));
