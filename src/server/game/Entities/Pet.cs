namespace Arise.Server.Entities;

public sealed class Pet : Companion
{
    internal Pet(int id)
        : base(new EntityId(EntityType.Pet, id))
    {
    }
}
