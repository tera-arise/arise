namespace Arise.Server.Entities;

public class Fixture : Unit
{
    internal Fixture(int id)
        : this(new EntityId(EntityType.Fixture, id))
    {
    }

    private protected Fixture(EntityId id)
        : base(id)
    {
    }
}
