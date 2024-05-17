// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Game.Net.Sessions;

namespace Arise.Client.Game.Net.Handlers;

internal sealed partial class GameClientPacketHandler
{
    public static void Handle(GameClientSession session, S_ARISE_WATCHDOG_REPORT packet)
    {
        // TODO
        throw new NotImplementedException();
    }
}
