using Arise.Server.World.Data;

namespace Arise.Server.World;

public static class WorldServiceCollectionExtensions
{
    public static IServiceCollection AddWorldServices(this IServiceCollection services)
    {
        return services
            .AddOptions<WorldOptions>()
            .BindConfiguration("World")
            .Services
            .AddSingleton<DataGraph>()
            .AddHostedService(static provider => provider.GetRequiredService<DataGraph>());
    }
}
