// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Server.Bridge;
using Arise.Server.Data;
using Arise.Server.Net;
using Arise.Server.Spatial;

namespace Arise.Server;

public static class GameServiceCollectionExtensions
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.TryAddSingleton(TimeProvider.System);

        return services
            .AddHostedService(static provider => provider.GetRequiredService<DataGraph>())
            .AddHostedService(static provider => provider.GetRequiredService<MapSpatialIndex>())
            .AddHostedService(static provider => provider.GetRequiredService<BridgeModuleGenerator>())
            .AddHostedService(static provider => provider.GetRequiredService<GameServer>())
            .AddAriseServerGame();
    }
}
