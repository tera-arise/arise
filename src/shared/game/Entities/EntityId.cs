namespace Arise.Entities;

public readonly struct EntityId :
    IEquatable<EntityId>,
    IEqualityOperators<EntityId, EntityId, bool>,
    IComparable<EntityId>,
    IComparisonOperators<EntityId, EntityId, bool>
{
    public EntityType Type { get; }

    public int Id { get; }

    public EntityId(EntityType type, int id)
    {
        Type = type;
        Id = id;
    }

    public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);

    public static bool operator !=(EntityId left, EntityId right) => !left.Equals(right);

    public static bool operator <(EntityId left, EntityId right) => left.CompareTo(right) < 0;

    public static bool operator <=(EntityId left, EntityId right) => left.CompareTo(right) <= 0;

    public static bool operator >(EntityId left, EntityId right) => left.CompareTo(right) > 0;

    public static bool operator >=(EntityId left, EntityId right) => left.CompareTo(right) >= 0;

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
