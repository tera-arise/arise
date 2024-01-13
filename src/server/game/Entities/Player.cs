namespace Arise.Server.Entities;

public sealed class Player : Unit
{
    internal Player(int id)
        : base(new(EntityType.Player, id))
    {
    }
}
