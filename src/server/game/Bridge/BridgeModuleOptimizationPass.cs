using dnlib.DotNet;

namespace Arise.Server.Bridge;

internal sealed class BridgeModuleOptimizationPass : BridgeModulePass
{
    [SuppressMessage("", "CA5394")]
    public override void Run(ModuleDefMD module, BridgeModuleKind kind, Random rng, GameOptions options)
    {
        foreach (var method in new MemberFinder().FindAll(module).MethodDefs.Keys)
        {
            if (method.Body is not { } body)
                continue;

            if (kind == BridgeModuleKind.Normal || rng.Next(0, 2) == 1)
                body.OptimizeBranches();
            else
                body.SimplifyBranches();

            if (kind == BridgeModuleKind.Normal || rng.Next(0, 2) == 1)
                body.OptimizeMacros();
            else
                body.SimplifyMacros(method.Parameters);
        }
    }
}
