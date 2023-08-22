namespace Arise.Server.Entities;

public abstract class Object : Entity
{
    private protected Object(EntityId id)
        : base(id)
    {
    }
}
