using Arise.Server.Bridge;
using Arise.Server.Data;
using Arise.Server.Net;
using Arise.Server.Net.Handlers;

namespace Arise.Server;

public static class WorldServiceCollectionExtensions
{
    public static IServiceCollection AddWorldServices(this IServiceCollection services)
    {
        return services
            .AddOptions<WorldOptions>()
            .BindConfiguration("World")
            .Services
            .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
            .AddSingleton<DataGraph>()
            .AddSingleton<BridgeModuleProvider>()
            .AddSingleton<GamePacketHandler>()
            .AddSingleton<GameServer>()
            .AddHostedService(static provider => provider.GetRequiredService<DataGraph>())
            .AddHostedService(static provider => provider.GetRequiredService<BridgeModuleProvider>())
            .AddHostedService(static provider => provider.GetRequiredService<GameServer>());
    }
}
