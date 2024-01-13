using dnlib.DotNet;

namespace Arise.Server.Bridge;

internal abstract class BridgeModulePass
{
    public abstract void Run(ModuleDefMD module, BridgeModuleKind kind, Random rng, GameOptions options);
}
