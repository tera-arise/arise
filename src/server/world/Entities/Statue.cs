namespace Arise.Server.Entities;

public sealed class Statue : Object
{
    internal Statue(int id)
        : base(new(EntityType.Shuttle, id))
    {
    }
}
