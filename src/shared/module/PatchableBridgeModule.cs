// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Bridge.Protection;

namespace Arise.Bridge;

public sealed class PatchableBridgeModule : BridgeModule
{
    public override PatchableBridgeDataComponent Data { get; } = new();

    public override PatchableBridgeProtectionComponent Protection { get; } = new();

    public override PatchableBridgeProtocolComponent Protocol { get; } = new();

    public override PatchableBridgeWatchdogComponent Watchdog { get; } = new();

    public PatchableBridgeModule()
    {
        GameProtection.Initialize();
    }
}
