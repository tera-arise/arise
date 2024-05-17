// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Bridge;

internal sealed class BridgeModuleAssemblyLoadContext : AssemblyLoadContext
{
    public BridgeModuleAssemblyLoadContext()
        : base(isCollectible: true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Load dependencies in the default context.
        return null;
    }
}
