namespace Arise.Server;

public sealed class WorldOptions : IOptions<WorldOptions>
{
    public int ConcurrentModules { get; } = 1;

    public Duration ModuleRotationTime { get; } = Duration.FromDays(1);

    public Duration ModuleValidityTime { get; } = Duration.FromDays(2);

    public ICollection<string> Endpoints { get; } = new List<string>();

    WorldOptions IOptions<WorldOptions>.Value => this;
}
