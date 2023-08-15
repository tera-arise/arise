using Arise.Net.Packets;

namespace Arise.Net.Serialization;

internal sealed class TeraGamePacketSerializer : GamePacketSerializer<TeraGamePacketCode, TeraGamePacket>
{
    public static TeraGamePacketSerializer Instance { get; } = new();

    private TeraGamePacketSerializer()
    {
    }

    protected override void GenerateDeserializer(Type type, ParameterExpression packet, ParameterExpression accessor)
    {
        // TODO
        throw new NotImplementedException();
    }

    protected override void GenerateSerializer(Type type, ParameterExpression packet, ParameterExpression accessor)
    {
        // TODO
        throw new NotImplementedException();
    }
}
