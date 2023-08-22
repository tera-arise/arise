namespace Arise.Server.Entities;

public sealed class GuildTower : Fixture
{
    internal GuildTower(int id)
        : base(new EntityId(EntityType.GuildTower, id))
    {
    }
}
