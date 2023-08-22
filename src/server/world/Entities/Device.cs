namespace Arise.Server.Entities;

public sealed class Device : Object
{
    internal Device(int id)
        : base(new(EntityType.Device, id))
    {
    }
}
