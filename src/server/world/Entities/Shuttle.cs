namespace Arise.Server.Entities;

public sealed class Shuttle : Object
{
    internal Shuttle(int id)
        : base(new(EntityType.Shuttle, id))
    {
    }
}
