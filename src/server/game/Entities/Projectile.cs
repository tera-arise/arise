namespace Arise.Server.Entities;

public sealed class Projectile : Object
{
    internal Projectile(int id)
        : base(new(EntityType.Projectile, id))
    {
    }
}
