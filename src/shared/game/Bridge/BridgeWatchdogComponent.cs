// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Bridge;

public abstract class BridgeWatchdogComponent
{
    public abstract void WriteReport(GameStreamAccessor accessor);
}
