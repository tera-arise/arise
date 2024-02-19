using Arise.Bridge;
using Arise.Net.Packets;

namespace Arise.Net.Sessions;

public abstract class GameSession
{
    public IPEndPoint EndPoint => _connection.EndPoint;

    public BridgeModule Module => _connection.Module;

    private readonly GameConnection _connection;

    protected GameSession(GameConnection connection)
    {
        _connection = connection;
    }

    public abstract GameSessionPacketPriority GetPriority(GamePacketCode code);

    private GameConnectionConduit GetConduit(GamePacket packet)
    {
        return GetPriority(packet.Code) switch
        {
            GameSessionPacketPriority.Low => _connection.LowPriority,
            GameSessionPacketPriority.Normal => _connection.NormalPriority,
            GameSessionPacketPriority.High => _connection.HighPriority,
            _ => throw new UnreachableException(),
        };
    }

    public void Post(GamePacket packet)
    {
        GetConduit(packet).PostPacket(packet);
    }

    public bool TryPost(GamePacket packet)
    {
        return GetConduit(packet).TryPostPacket(packet);
    }

    public ValueTask SendAsync(GamePacket packet)
    {
        return GetConduit(packet).SendPacketAsync(packet);
    }

    public ValueTask<bool> TrySendAsync(GamePacket packet)
    {
        return GetConduit(packet).TrySendPacketAsync(packet);
    }

    public ValueTask DisconnectAsync()
    {
        return _connection.DisposeAsync();
    }
}
