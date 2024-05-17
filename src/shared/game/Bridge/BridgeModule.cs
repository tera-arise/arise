// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Bridge;

public abstract class BridgeModule
{
    public abstract BridgeDataComponent Data { get; }

    public abstract BridgeProtectionComponent Protection { get; }

    public abstract BridgeProtocolComponent Protocol { get; }

    public abstract BridgeWatchdogComponent Watchdog { get; }
}
