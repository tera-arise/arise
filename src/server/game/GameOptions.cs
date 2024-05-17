// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server;

internal sealed class GameOptions : IOptions<GameOptions>
{
    public int ConcurrentModules { get; } = 1;

    public TimeSpan ModuleRotationTime { get; } = TimeSpan.FromDays(1);

    public TimeSpan ModuleValidityTime { get; } = TimeSpan.FromDays(2);

    public TimeSpan SpatialDataPollingTime { get; } = TimeSpan.FromMinutes(15);

    public TimeSpan SpatialDataRetentionTime { get; } = TimeSpan.FromHours(1);

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
