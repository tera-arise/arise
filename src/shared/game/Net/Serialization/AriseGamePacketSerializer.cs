using Arise.Entities;
using Arise.Net.Packets;
using static DotNext.Metaprogramming.CodeGenerator;

namespace Arise.Net.Serialization;

internal sealed class AriseGamePacketSerializer : GamePacketSerializer<AriseGamePacketCode, AriseGamePacket>
{
    public static AriseGamePacketSerializer Instance { get; }

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

    private static readonly MethodInfo _readCompactEnum =
        typeof(GameStreamAccessor).GetMethod("ReadCompactEnum", 1, [])!;

    private static readonly MethodInfo _writeCompactEnum =
        typeof(GameStreamAccessor).GetMethod("WriteCompactEnum", 1, [Type.MakeGenericMethodParameter(0)])!;

    private static readonly MethodInfo _createBuilder =
        typeof(ImmutableArray).GetMethod("CreateBuilder", 1, [typeof(int)])!;

    static AriseGamePacketSerializer()
    {
        // Must be done here rather than at the declaration to resolve initialization ordering issues.
        Instance = new();
    }

    private AriseGamePacketSerializer()
    {
    }

    protected override void GenerateDeserializer(Expression packet, Expression accessor)
    {
        void GenerateForObject(Expression @object)
        {
            void GenerateForValue(Type type, Action<Expression> handler)
            {
                Expression result;

                if (IsSimpleType(type) || type == typeof(string))
                    result = accessor.Call((_compactTypes.Contains(type) ? "ReadCompact" : "Read") + type.Name);
                else if (type.IsEnum)
                    result = accessor.Call(_readCompactEnum.MakeGenericMethod(type));
                else if (type == typeof(ReadOnlyMemory<byte>))
                {
                    var memory = Variable(type, "memory");
                    var count = Variable(typeof(ushort), "count");

                    Assign(count, accessor.Call("ReadCompactUInt16"));
                    If(count.NotEqual(((ushort)0).Const()))
                        .Then(() =>
                        {
                            var array = Variable(typeof(byte[]), "array");

                            Assign(array, typeof(byte[]).New(count.Convert<int>()));
                            Call(accessor, "Read", array.Convert(typeof(Span<byte>)));
                            Assign(memory, array.Convert<ReadOnlyMemory<byte>>());
                        })
                        .Else(() => Assign(memory, ReadOnlyMemory<byte>.Empty.Const()))
                        .End();

                    result = memory;
                }
                else if (IsArrayType(type))
                {
                    var elemType = type.GetGenericArguments()[0];

                    var array = Variable(type, "array");
                    var builder = Variable(typeof(ImmutableArray<>.Builder).MakeGenericType(elemType), "builder");
                    var i = Variable(typeof(int), "i");

                    Assign(
                        builder,
                        Expression.Call(
                            _createBuilder.MakeGenericMethod(elemType),
                            accessor.Call("ReadCompactUInt16").Convert<int>()));
                    For(
                        i.Assign(0.Const()),
                        i => i.LessThan(builder.Property("Capacity")),
                        static i => PreIncrementAssign(i),
                        i => GenerateForValue(elemType, value => builder.Call("Add", value)));
                    Assign(array, builder.Call("MoveToImmutable"));

                    result = array;
                }
                else if (type.IsValueType)
                {
                    var obj = Variable(type, "obj");

                    Assign(obj, type.New());

                    GenerateForObject(obj);

                    result = obj;
                }
                else
                    throw new UnreachableException();

                handler(result);
            }

            foreach (var prop in EnumerateProperties(@object.Type))
                GenerateForValue(prop.PropertyType, value => Assign(@object.Property(prop), value));
        }

        GenerateForObject(packet);
    }

    protected override void GenerateSerializer(Expression packet, Expression accessor)
    {
        void GenerateForObject(Expression @object)
        {
            void GenerateForValue(Expression value)
            {
                var type = value.Type;

                if (IsSimpleType(type) || type == typeof(string))
                    Call(accessor, (_compactTypes.Contains(type) ? "WriteCompact" : "Write") + type.Name, value);
                else if (type.IsEnum)
                    Call(accessor, _writeCompactEnum.MakeGenericMethod(type), value);
                else if (type == typeof(ReadOnlyMemory<byte>))
                {
                    Call(accessor, "WriteCompactUInt16", value.Property("Length").Convert<ushort>());
                    Call(accessor, "Write", value.Property("Span"));
                }
                else if (IsArrayType(type))
                {
                    Call(accessor, "WriteCompactUInt16", value.Property("Length").Convert<ushort>());
                    ForEach(value, elem => GenerateForValue(elem));
                }
                else if (type.IsValueType)
                    GenerateForObject(value);
                else
                    throw new UnreachableException();
            }

            foreach (var prop in EnumerateProperties(@object.Type))
                GenerateForValue(@object.Property(prop));
        }

        GenerateForObject(packet);
    }
}
