namespace Arise.Server;

public sealed class WorldOptions : IOptions<WorldOptions>
{
    public ICollection<string> Endpoints { get; } = new List<string>();

    WorldOptions IOptions<WorldOptions>.Value => this;
}
