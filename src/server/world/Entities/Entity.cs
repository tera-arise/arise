namespace Arise.Server.Entities;

public abstract class Entity
{
    public EntityId Id { get; }

    private protected Entity(EntityId id)
    {
        Id = id;
    }
}
