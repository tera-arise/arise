namespace Arise.Server.Entities;

public sealed class ResourceNode : Object
{
    internal ResourceNode(int id)
        : base(new(EntityType.ResourceNode, id))
    {
    }
}
