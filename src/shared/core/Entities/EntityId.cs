namespace Arise.Entities;

public readonly struct EntityId :
    IEquatable<EntityId>,
    IEqualityOperators<EntityId, EntityId, bool>,
    IComparable<EntityId>,
    IComparisonOperators<EntityId, EntityId, bool>
{
    public int Id { get; }

    public EntityType Type { get; }

    internal EntityId(int id, EntityType type)
    {
        Id = id;
        Type = type;
    }

    public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);

    public static bool operator !=(EntityId left, EntityId right) => !left.Equals(right);

    public static bool operator <(EntityId left, EntityId right) => left.CompareTo(right) < 0;

    public static bool operator <=(EntityId left, EntityId right) => left.CompareTo(right) <= 0;

    public static bool operator >(EntityId left, EntityId right) => left.CompareTo(right) > 0;

    public static bool operator >=(EntityId left, EntityId right) => left.CompareTo(right) >= 0;

    internal static EntityId FromRaw(long value)
    {
        return new((int)Bits.Extract(value, 0, 32), (EntityType)Bits.Extract(value, 32, 32));
    }

    internal long ToRaw()
    {
        return Bits.Join([(Id, 0, 32), ((int)Type, 32, 32)]);
    }

    public bool Equals(EntityId other)
    {
        return (Type, Id) == (other.Type, other.Id);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is EntityId id && Equals(id);
    }

    public int CompareTo(EntityId other)
    {
        return Type != other.Type ? Type.CompareTo(other.Type) : Id.CompareTo(other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Id);
    }

    public override string ToString()
    {
        return $"({Type}: {Id})";
    }
}
