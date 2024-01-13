namespace Arise.Server.Entities;

public sealed class RandomBox : Object
{
    internal RandomBox(int id)
        : base(new(EntityType.RandomBox, id))
    {
    }
}
