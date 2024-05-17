// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Entities;
using Arise.Net.Packets;
using static DotNext.Metaprogramming.CodeGenerator;

namespace Arise.Net.Serialization;

internal abstract class GamePacketSerializer<TPacket>
    where TPacket : GamePacket
{
    private static readonly FrozenSet<Type> _extraSimpleTypes = new[]
    {
        typeof(Vector3),
        typeof(EntityId),
    }.ToFrozenSet();

    private static readonly MethodInfo _as = typeof(Unsafe).GetMethod("As", 1, [Type.MakeGenericMethodParameter(0)])!;

    private readonly FrozenDictionary<GamePacketCode, Func<TPacket>> _creators;

    private readonly FrozenDictionary<GamePacketCode, Action<TPacket, GameStreamAccessor>> _deserializers;

    private readonly FrozenDictionary<GamePacketCode, Action<TPacket, GameStreamAccessor>> _serializers;

    private int _variableCounter;

    private protected GamePacketSerializer()
    {
        var creators = new Dictionary<GamePacketCode, Func<TPacket>>();
        var deserializers = new Dictionary<GamePacketCode, Action<TPacket, GameStreamAccessor>>();
        var serializers = new Dictionary<GamePacketCode, Action<TPacket, GameStreamAccessor>>();

        Action<TPacket, GameStreamAccessor> CompileFunction(Type type, Action<Expression, Expression> generator)
        {
            return Lambda<Action<TPacket, GameStreamAccessor>>(ctx =>
            {
                var (packet, accessor) = ctx;

                var packet2 = Variable(type, "packet");

                Assign(packet2, Expression.Call(_as.MakeGenericMethod(type), packet));

                generator(packet2, accessor);
            }).Compile();
        }

        // TODO: This probably needs to be parallelized as we add more packet definitions.
        foreach (var type in typeof(ThisAssembly)
            .Assembly
            .DefinedTypes
            .Where(static type => !type.IsAbstract && type.IsSubclassOf(typeof(TPacket))))
        {
            var packet = Unsafe.As<TPacket>(Activator.CreateInstance(type)!);

            creators.Add(packet.Code, Lambda<Func<TPacket>>(_ => type.New()).Compile());
            deserializers.Add(packet.Code, CompileFunction(type, GenerateDeserializer));
            serializers.Add(packet.Code, CompileFunction(type, GenerateSerializer));
        }

        _creators = creators.ToFrozenDictionary();
        _deserializers = deserializers.ToFrozenDictionary();
        _serializers = serializers.ToFrozenDictionary();
    }

    protected static IEnumerable<PropertyInfo> EnumerateProperties(Type type)
    {
        var isPacket = type.IsSubclassOf(typeof(GamePacket));

        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(prop => !isPacket || prop.Name != "Code")
            .OrderBy(static prop => prop.MetadataToken);
    }

    protected static bool IsSimpleType(Type type)
    {
        return (type.IsPrimitive && type != typeof(nuint) && type != typeof(nint)) || _extraSimpleTypes.Contains(type);
    }

    protected static bool IsArrayType(Type type)
    {
        return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableArray<>);
    }

    protected ParameterExpression Variable(Type type, string name)
    {
        return DeclareVariable(type, name + _variableCounter++);
    }

    protected abstract void GenerateDeserializer(Expression packet, Expression accessor);

    protected abstract void GenerateSerializer(Expression packet, Expression accessor);

    public TPacket? CreatePacket(GamePacketCode code)
    {
        return _creators.TryGetValue(code, out var creator) ? creator() : null;
    }

    public void DeserializePacket(TPacket packet, GameStreamAccessor accessor)
    {
        try
        {
            _deserializers[packet.Code](packet, accessor);
        }
        catch (EndOfStreamException)
        {
            throw ExceptionDispatchInfo.SetCurrentStackTrace(new InvalidDataException());
        }
    }

    public void SerializePacket(TPacket packet, GameStreamAccessor accessor)
    {
        _serializers[packet.Code](packet, accessor);
    }
}
