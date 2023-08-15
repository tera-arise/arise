using Arise.Net.Packets;
using static DotNext.Metaprogramming.CodeGenerator;

namespace Arise.Net.Serialization;

internal abstract class GamePacketSerializer<TCode, TPacket>
    where TCode : unmanaged, Enum
    where TPacket : GamePacket<TCode>
{
    private readonly FrozenDictionary<TCode, Func<TPacket>> _creators;

    private readonly FrozenDictionary<TCode, Action<object, GameStreamAccessor>> _deserializers;

    private readonly FrozenDictionary<TCode, Action<object, GameStreamAccessor>> _serializers;

    private protected GamePacketSerializer()
    {
        var creators = new Dictionary<TCode, Func<TPacket>>();
        var deserializers = new Dictionary<TCode, Action<object, GameStreamAccessor>>();
        var serializers = new Dictionary<TCode, Action<object, GameStreamAccessor>>();

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

    protected static Action<object, GameStreamAccessor> CompileFunction(
        Type type,
        Action<Type, ParameterExpression, ParameterExpression> generator)
    {
        return Lambda<Action<object, GameStreamAccessor>>(ctx =>
        {
            var (packet, accessor) = ctx;

            generator(type, packet, accessor);
        }).Compile();
    }

    protected abstract void GenerateDeserializer(Type type, ParameterExpression packet, ParameterExpression accessor);

    protected abstract void GenerateSerializer(Type type, ParameterExpression packet, ParameterExpression accessor);

    public TPacket? CreatePacket(TCode code)
    {
        return _creators.TryGetValue(code, out var creator) ? creator() : null;
    }

    public void DeserializePacket(TPacket packet, GameStreamAccessor accessor)
    {
        _deserializers[packet.Code](packet, accessor);
    }

    public void SerializePacket(TPacket packet, GameStreamAccessor accessor)
    {
        _serializers[packet.Code](packet, accessor);
    }
}
