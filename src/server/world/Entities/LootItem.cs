namespace Arise.Server.Entities;

public sealed class LootItem : Object
{
    internal LootItem(int id)
        : base(new(EntityType.LootItem, id))
    {
    }
}
