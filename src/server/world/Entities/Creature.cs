namespace Arise.Server.Entities;

public class Creature : Unit
{
    internal Creature(int id)
        : this(new EntityId(EntityType.Creature, id))
    {
    }

    private protected Creature(EntityId id)
        : base(id)
    {
    }
}
