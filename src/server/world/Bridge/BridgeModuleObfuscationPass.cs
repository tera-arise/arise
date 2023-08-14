using dnlib.DotNet;

namespace Arise.Server.Bridge;

internal sealed class BridgeModuleObfuscationPass : BridgeModulePass
{
    public override void Run(ModuleDefMD module, BridgeModuleKind kind, Random rng, WorldOptions options)
    {
        if (kind == BridgeModuleKind.Server)
            return;

        // TODO
    }
}
