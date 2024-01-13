using Arise.Daemon;

return await DaemonHost.RunAsync(
    args,
    ["game", "gateway"],
    static services =>
        services
            .AddGameServices()
            .AddGatewayServices(),
    static logger => logger.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning),
    static host => host.ConfigureGatewayHost(static app => app.UseSerilogRequestLogging()));
