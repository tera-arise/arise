// SPDX-License-Identifier: AGPL-3.0-or-later

using dnlib.DotNet;

namespace Arise.Server.Bridge;

internal sealed class BridgeModuleObfuscationPass : BridgeModulePass
{
    public override void Run(ModuleDefMD module, BridgeModuleKind kind, Random rng, GameOptions options)
    {
        if (kind == BridgeModuleKind.Normal)
            return;

        // TODO
    }
}
