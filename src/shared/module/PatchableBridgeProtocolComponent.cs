// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Bridge;

public sealed class PatchableBridgeProtocolComponent : BridgeProtocolComponent
{
    [Obfuscation]
    protected override void InitializeCodes(Dictionary<GamePacketCode, ushort> codes)
    {
        // Filled in by the server's BridgeModuleGenerator.

        codes.Add(default, 42);
    }
}
