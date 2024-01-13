namespace Arise.Server.Entities;

public sealed class Door : Object
{
    internal Door(int id)
        : base(new(EntityType.Door, id))
    {
    }
}
