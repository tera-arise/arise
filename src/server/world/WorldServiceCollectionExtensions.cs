namespace Arise.Server;

public static class WorldServiceCollectionExtensions
{
    internal static void AddHostedSingleton<T>(this IServiceCollection services)
        where T : class, IHostedService
    {
        _ = services
            .AddSingleton<T>()
            .AddHostedService(static provider => provider.GetRequiredService<T>());
    }

    public static IServiceCollection AddWorldServices(this IServiceCollection services)
    {
        return services
            .AddOptions<WorldOptions>()
            .BindConfiguration("World")
            .Services
            .AddAriseServerWorld();
    }
}
