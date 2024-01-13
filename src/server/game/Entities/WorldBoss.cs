namespace Arise.Server.Entities;

public sealed class WorldBoss : Boss
{
    internal WorldBoss(int id)
        : base(new EntityId(EntityType.WorldBoss, id))
    {
    }
}
