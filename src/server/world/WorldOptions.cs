namespace Arise.Server;

public sealed class WorldOptions : IOptions<WorldOptions>
{
    public ICollection<IPEndPoint> Endpoints { get; } = new List<IPEndPoint>();

    WorldOptions IOptions<WorldOptions>.Value => this;
}
