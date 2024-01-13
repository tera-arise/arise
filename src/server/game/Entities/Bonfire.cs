namespace Arise.Server.Entities;

public sealed class Bonfire : Object
{
    internal Bonfire(int id)
        : base(new(EntityType.Bonfire, id))
    {
    }
}
