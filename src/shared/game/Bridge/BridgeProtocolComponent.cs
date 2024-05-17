// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Net.Packets;

namespace Arise.Bridge;

public abstract class BridgeProtocolComponent
{
    public FrozenDictionary<GamePacketCode, ushort> RealToSession { get; }

    public FrozenDictionary<ushort, GamePacketCode> SessionToReal { get; }

    [SuppressMessage("", "CA2214")]
    protected BridgeProtocolComponent()
    {
        var codes = new Dictionary<GamePacketCode, ushort>();

        InitializeCodes(codes);

        RealToSession = codes.ToFrozenDictionary();
        SessionToReal = codes.ToFrozenDictionary(static kvp => kvp.Value, static kvp => kvp.Key);
    }

    protected abstract void InitializeCodes(Dictionary<GamePacketCode, ushort> codes);
}
