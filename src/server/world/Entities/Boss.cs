namespace Arise.Server.Entities;

public class Boss : Creature
{
    internal Boss(int id)
        : this(new EntityId(EntityType.Boss, id))
    {
    }

    private protected Boss(EntityId id)
        : base(id)
    {
    }
}
