using Arise.Entities;
using Arise.Net.Packets;
using static DotNext.Metaprogramming.CodeGenerator;

namespace Arise.Net.Serialization;

internal sealed class AriseGamePacketSerializer : GamePacketSerializer<AriseGamePacketCode, AriseGamePacket>
{
    public static AriseGamePacketSerializer Instance { get; }

    private static readonly FrozenSet<Type> _extraSimpleTypes = new[]
    {
        typeof(string),
        typeof(Vector3),
        typeof(EntityId),
    }.ToFrozenSet();

    private static readonly FrozenSet<Type> _compactTypes = new[]
    {
        typeof(ushort),
        typeof(short),
        typeof(uint),
        typeof(int),
        typeof(ulong),
        typeof(long),
        typeof(char),
        typeof(string),
        typeof(EntityId),
    }.ToFrozenSet();

    private static readonly MethodInfo _readEnum =
        typeof(GameStreamAccessor).GetMethod("ReadCompactEnum", 1, Type.EmptyTypes)!;

    private static readonly MethodInfo _writeEnum =
        typeof(GameStreamAccessor).GetMethod("WriteCompactEnum", 1, new[] { Type.MakeGenericMethodParameter(0) })!;

    private int _variableCounter;

    static AriseGamePacketSerializer()
    {
        Instance = new();
    }

    private AriseGamePacketSerializer()
    {
    }

    private static IEnumerable<PropertyInfo> EnumerateProperties(Type type)
    {
        var isPacket = type.IsSubclassOf(typeof(GamePacket));

        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(prop => !isPacket || prop.Name != "Code")
            .OrderBy(static prop => prop.MetadataToken);
    }

    private static bool IsSimpleType(Type type)
    {
        return (type.IsPrimitive && type != typeof(nuint) && type != typeof(nint)) || _extraSimpleTypes.Contains(type);
    }

    private static bool IsListType(Type type)
    {
        return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    private string GenerateName(string name)
    {
        return name + _variableCounter++;
    }

    protected override void GenerateDeserializer(Expression packet, Expression accessor)
    {
        void GenerateFor(Type type, Action<Expression> assign)
        {
            if (IsSimpleType(type))
                assign(accessor.Call((_compactTypes.Contains(type) ? "ReadCompact" : "Read") + type.Name));
            else if (type.IsEnum)
                assign(accessor.Call(_readEnum.MakeGenericMethod(type)));
            else if (type == typeof(byte[]))
            {
                var array = DeclareVariable<byte[]>(GenerateName("array"));

                Assign(array, type.New(accessor.Call("ReadCompactUInt16").Convert<int>()));
                Call(accessor, "Read", array.Convert(typeof(Span<byte>)));

                assign(array);
            }
            else if (IsListType(type))
            {
                var elemType = type.GetGenericArguments()[0];

                var list = DeclareVariable(type, GenerateName("list"));
                var i = DeclareVariable<int>(GenerateName("i"));

                Assign(list, type.New(accessor.Call("ReadCompactUInt16").Convert<int>()));
                For(
                    i.Assign(0.Const()),
                    i => i.LessThan(list.Property("Capacity")),
                    static i => PostIncrementAssign(i),
                    i =>
                    {
                        var item = DeclareVariable(elemType, GenerateName("item"));

                        GenerateFor(elemType, value => Assign(list.Property("Item", i), value));
                    });

                assign(list);
            }
            else if (type.IsValueType)
            {
                var obj = DeclareVariable(type, GenerateName("obj"));

                foreach (var objProp in EnumerateProperties(type))
                    GenerateFor(objProp.PropertyType, value => Assign(obj.Property(objProp), value));

                assign(obj);
            }
            else
                throw new UnreachableException();
        }

        foreach (var prop in EnumerateProperties(packet.Type))
            GenerateFor(prop.PropertyType, value => Assign(packet.Property(prop), value));
    }

    protected override void GenerateSerializer(Expression packet, Expression accessor)
    {
        void GenerateFor(Expression value)
        {
            var type = value.Type;

            if (IsSimpleType(type))
                Call(accessor, (_compactTypes.Contains(type) ? "WriteCompact" : "Write") + type.Name, value);
            else if (type.IsEnum)
                Call(accessor, _writeEnum.MakeGenericMethod(type), value);
            else if (type == typeof(byte[]))
            {
                Call(accessor, "WriteCompactUInt16", value.ArrayLength().Convert<ushort>());
                Call(accessor, "Write", value.Convert(typeof(ReadOnlySpan<byte>)));
            }
            else if (IsListType(type))
            {
                Call(accessor, "WriteCompactUInt16", value.Property("Count").Convert<ushort>());
                ForEach(value, elem => GenerateFor(elem));
            }
            else if (type.IsValueType)
            {
                foreach (var objProp in EnumerateProperties(type))
                    GenerateFor(value.Property(objProp));
            }
            else
                throw new UnreachableException();
        }

        foreach (var prop in EnumerateProperties(packet.Type))
            GenerateFor(packet.Property(prop));
    }
}
