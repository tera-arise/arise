namespace Arise.Server;

internal sealed class GameOptions : IOptions<GameOptions>
{
    public int ConcurrentModules { get; } = 1;

    public Duration ModuleRotationTime { get; } = Duration.FromDays(1);

    public Duration ModuleValidityTime { get; } = Duration.FromDays(2);

    public Duration SpatialDataPollingTime { get; } = Duration.FromMinutes(15);

    public Duration SpatialDataRetentionTime { get; } = Duration.FromHours(1);

    public ICollection<string> Endpoints { get; } = [];

    GameOptions IOptions<GameOptions>.Value => this;

    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        _ = services
            .AddOptions<GameOptions>()
            .BindConfiguration("Game");
    }
}
