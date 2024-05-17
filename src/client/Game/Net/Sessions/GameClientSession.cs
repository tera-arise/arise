// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Game.Net.Sessions;

internal sealed class GameClientSession : GameSession
{
    public GameClientSession(GameConnection connection)
        : base(connection)
    {
    }

    public override GameSessionPacketPriority GetPriority(GamePacketCode code)
    {
        // TODO: Actually sort packet codes into low/normal/high priority categories.
        return GameSessionPacketPriority.Normal;
    }
}
