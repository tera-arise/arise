using Arise.Net.Packets;

namespace Arise.Net.Sessions;

public readonly struct GameSessionPort
{
    private readonly GameConnectionConduit _conduit;

    internal GameSessionPort(GameConnectionConduit conduit)
    {
        _conduit = conduit;
    }

    public void Post(GamePacket packet)
    {
        _conduit.PostPacket(packet);
    }

    public bool TryPost(GamePacket packet)
    {
        return _conduit.TryPostPacket(packet);
    }

    public ValueTask SendAsync(GamePacket packet)
    {
        return _conduit.SendPacketAsync(packet);
    }

    public ValueTask<bool> TrySendAsync(GamePacket packet)
    {
        return _conduit.TrySendPacketAsync(packet);
    }
}
