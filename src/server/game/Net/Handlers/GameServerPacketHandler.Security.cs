// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Server.Net.Sessions;

namespace Arise.Server.Net.Handlers;

internal sealed partial class GameServerPacketHandler
{
    public static void Handle(GameServerSession session, C_ARISE_WATCHDOG_REPORT packet)
    {
        // TODO
        throw new NotImplementedException();
    }
}
