using Arise.Server.Bridge;
using Arise.Server.Data;
using Arise.Server.Net;
using Arise.Server.Spatial;

namespace Arise.Server;

public static class WorldServiceCollectionExtensions
{
    public static IServiceCollection AddWorldServices(this IServiceCollection services)
    {
        return services
            .AddOptions<WorldOptions>()
            .BindConfiguration("World")
            .Services
            .AddHostedService(static provider => provider.GetRequiredService<DataGraph>())
            .AddHostedService(static provider => provider.GetRequiredService<MapSpatialIndex>())
            .AddHostedService(static provider => provider.GetRequiredService<BridgeModuleGenerator>())
            .AddHostedService(static provider => provider.GetRequiredService<GameServer>())
            .AddAriseServerWorld();
    }
}
