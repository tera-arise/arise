// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Net.Sessions;

internal sealed class GameServerSession : GameSession
{
    // TODO: Add important state (AccountDocument, Player, etc).

    public GameServerSession(GameConnection connection)
        : base(connection)
    {
    }

    public override GameSessionPacketPriority GetPriority(GamePacketCode code)
    {
        // TODO: Actually sort packet codes into low/normal/high priority categories.
        return GameSessionPacketPriority.Normal;
    }
}
