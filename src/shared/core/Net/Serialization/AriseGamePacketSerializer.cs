using Arise.Net.Packets;

namespace Arise.Net.Serialization;

internal sealed class AriseGamePacketSerializer : GamePacketSerializer<AriseGamePacketCode, AriseGamePacket>
{
    public static AriseGamePacketSerializer Instance { get; } = new();

    private AriseGamePacketSerializer()
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
