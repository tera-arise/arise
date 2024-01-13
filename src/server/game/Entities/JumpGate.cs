namespace Arise.Server.Entities;

public sealed class JumpGate : Object
{
    internal JumpGate(int id)
        : base(new(EntityType.JumpGate, id))
    {
    }
}
