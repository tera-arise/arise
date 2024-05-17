// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Daemon;

return await DaemonHost.RunAsync(
    args,
    ["gateway"],
    static services => services.AddGatewayServices(),
    static logger => logger.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning),
    static host => host.ConfigureGatewayHost(static app => app.UseSerilogRequestLogging()));
