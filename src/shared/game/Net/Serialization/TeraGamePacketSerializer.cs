using Arise.Net.Packets;
using static DotNext.Metaprogramming.CodeGenerator;

namespace Arise.Net.Serialization;

internal sealed class TeraGamePacketSerializer : GamePacketSerializer<TeraGamePacket>
{
    public static TeraGamePacketSerializer Instance { get; }

    private static readonly MethodInfo _readEnum = typeof(GameStreamAccessor).GetMethod("ReadEnum", 1, [])!;

    private static readonly MethodInfo _writeEnum =
        typeof(GameStreamAccessor).GetMethod("WriteEnum", 1, [Type.MakeGenericMethodParameter(0)])!;

    private static readonly MethodInfo _readPacketOffset =
        typeof(GameStreamAccessor).GetMethod("ReadPacketOffset", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo _writePacketOffset =
        typeof(GameStreamAccessor).GetMethod("WritePacketOffset", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo _createBuilder =
        typeof(ImmutableArray).GetMethod("CreateBuilder", 1, [typeof(int)])!;

    static TeraGamePacketSerializer()
    {
        // Must be done here rather than at the declaration to resolve initialization ordering issues.
        Instance = new();
    }

    private TeraGamePacketSerializer()
    {
    }

    protected override void GenerateDeserializer(Expression packet, Expression accessor)
    {
        void GenerateForObject(Expression @object)
        {
            void GenerateForValue(Type type, Action<Expression> handler)
            {
                Expression result;

                if (IsSimpleType(type))
                    result = accessor.Call("Read" + type.Name);
                else if (type.IsEnum)
                    result = accessor.Call(_readEnum.MakeGenericMethod(type));
                else if (type == typeof(string))
                {
                    var str = Variable(type, "str");
                    var offset = Variable(typeof(ushort), "offset");
                    var position = Variable(typeof(long), "position");

                    Assign(offset, accessor.Call(_readPacketOffset));
                    Assign(position, accessor.Property("Position"));
                    Assign(accessor.Property("Position"), offset.Convert<long>());
                    Assign(str, accessor.Call("ReadString"));
                    Assign(accessor.Property("Position"), position);

                    result = str;
                }
                else if (type == typeof(ReadOnlyMemory<byte>))
                {
                    var memory = Variable(type, "memory");
                    var offset = Variable(typeof(ushort), "offset");
                    var count = Variable(typeof(ushort), "count");

                    Assign(offset, accessor.Call(_readPacketOffset));
                    Assign(count, accessor.Call("ReadUInt16"));
                    If(count.NotEqual(((ushort)0).Const()))
                        .Then(() =>
                        {
                            var array = Variable(typeof(byte[]), "array");
                            var position = Variable(typeof(long), "position");

                            Assign(position, accessor.Property("Position"));
                            Assign(accessor.Property("Position"), offset.Convert<long>());
                            Assign(array, typeof(byte[]).New(count.Convert<int>()));
                            Call(accessor, "Read", array.Convert(typeof(Span<byte>)));
                            Assign(accessor.Property("Position"), position);
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
                    var builder = Variable(
                        typeof(ImmutableArray<>.Builder).MakeGenericType(elemType), "builder");
                    var offset = Variable(typeof(ushort), "offset");

                    Assign(
                        builder,
                        Expression.Call(
                            _createBuilder.MakeGenericMethod(elemType), accessor.Call("ReadUInt16").Convert<int>()));
                    Assign(offset, accessor.Call(_readPacketOffset));
                    If(builder.Property("Capacity").NotEqual(0.Const()))
                        .Then(() =>
                        {
                            var position = Variable(typeof(long), "position");
                            var i = Variable(typeof(int), "i");

                            Assign(position, accessor.Property("Position"));
                            Assign(accessor.Property("Position"), offset.Convert<long>());
                            For(
                                i.Assign(0.Const()),
                                i => i.LessThan(builder.Property("Capacity")),
                                static i => PreIncrementAssign(i),
                                i => GenerateForValue(elemType, value => builder.Call("Add", value)));
                            Assign(accessor.Property("Position"), position);
                        })
                        .End();
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
            void GenerateFirstPassForValue(Expression value, Action<ParameterExpression> handler)
            {
                var type = value.Type;

                if (IsSimpleType(type))
                    Call(accessor, "Write" + type.Name, value);
                else if (type.IsEnum)
                    Call(accessor, _writeEnum.MakeGenericMethod(type), value);
                else if (type == typeof(string))
                {
                    var offset = Variable(typeof(long), "offset");

                    handler(offset);

                    // For strings, we always come back and update the offset. This is because even empty strings need
                    // to write a terminator. So just skip writing a zero here.
                    Assign(offset, accessor.Property("Position"));
                    Assign(accessor.Property("Position"), offset.Add(((long)sizeof(ushort)).Const()));
                }
                else if (type == typeof(ReadOnlyMemory<byte>))
                {
                    var offset = Variable(typeof(long), "offset");

                    handler(offset);

                    Assign(offset, accessor.Property("Position"));
                    Call(accessor, "WriteUInt16", ((ushort)0).Const());
                    Call(accessor, "WriteUInt16", value.Property("Length").Convert<ushort>());
                }
                else if (IsArrayType(type))
                {
                    var offset = Variable(typeof(long), "offset");

                    handler(offset);

                    Call(accessor, "WriteUInt16", value.Property("Length").Convert<ushort>());
                    Assign(offset, accessor.Property("Position"));
                    Call(accessor, "WriteUInt16", ((ushort)0).Const());
                }
                else if (type.IsValueType)
                    GenerateForObject(value);
                else
                    throw new UnreachableException();
            }

            void GenerateSecondPassForValue(Expression value, ParameterExpression offset)
            {
                var type = value.Type;

                if (type == typeof(string))
                {
                    var position = Variable(typeof(long), "position");

                    Assign(position, accessor.Property("Position"));
                    Assign(accessor.Property("Position"), offset);
                    Call(accessor, _writePacketOffset, position.Convert<ushort>());
                    Assign(accessor.Property("Position"), position);
                    Call(accessor, "WriteString", value);
                }
                else if (type == typeof(ReadOnlyMemory<byte>))
                    If(value.Property("Length").NotEqual(0.Const()))
                        .Then(() =>
                        {
                            var position = Variable(typeof(long), "position");

                            Assign(position, accessor.Property("Position"));
                            Assign(accessor.Property("Position"), offset);
                            Call(accessor, _writePacketOffset, position.Convert<ushort>());
                            Assign(accessor.Property("Position"), position);
                            Call(accessor, "Write", value.Property("Span"));
                        })
                        .End();
                else if (IsArrayType(type))
                    If(value.Property("Length").NotEqual(0.Const()))
                        .Then(() =>
                        {
                            var position = Variable(typeof(long), "position");
                            var i = Variable(typeof(int), "i");

                            Assign(position, accessor.Property("Position"));
                            Assign(accessor.Property("Position"), offset);
                            Call(accessor, _writePacketOffset, position.Convert<ushort>());
                            Assign(accessor.Property("Position"), position);
                            Assign(i, 0.Const());
                            ForEach(
                                value,
                                item =>
                                {
                                    var position2 = Variable(typeof(long), "position2");

                                    Assign(position2, accessor.Property("Position"));
                                    Call(accessor, _writePacketOffset, position2.Convert<ushort>());
                                    Call(accessor, "WriteUInt16", ((ushort)0).Const());

                                    GenerateFirstPassForValue(item, offset => GenerateSecondPassForValue(item, offset));

                                    If(i.NotEqual(value.Property("Length").Subtract(1.Const())))
                                        .Then(() =>
                                        {
                                            var position3 = Variable(typeof(long), "position3");

                                            Assign(position3, accessor.Property("Position"));
                                            Assign(
                                                accessor.Property("Position"),
                                                position2.Add(((long)sizeof(ushort)).Const()));
                                            Call(accessor, _writePacketOffset, position3.Convert<ushort>());
                                            Assign(accessor.Property("Position"), position3);
                                            PreIncrementAssign(i);
                                        })
                                        .End();
                                });
                        })
                        .End();
                else
                    throw new UnreachableException();
            }

            var offsets = new List<(Expression, ParameterExpression)>();

            foreach (var prop in EnumerateProperties(@object.Type))
            {
                var value = @object.Property(prop);

                GenerateFirstPassForValue(value, offset => offsets.Add((value, offset)));
            }

            foreach (var (propValue, offset) in offsets)
                GenerateSecondPassForValue(propValue, offset);
        }

        GenerateForObject(packet);
    }
}
