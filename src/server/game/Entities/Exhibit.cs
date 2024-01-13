namespace Arise.Server.Entities;

public sealed class Exhibit : Object
{
    internal Exhibit(int id)
        : base(new(EntityType.Exhibit, id))
    {
    }
}
