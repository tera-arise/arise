// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Bridge;

public static class BridgeModuleActivator
{
    public static BridgeModule Create(ReadOnlyMemory<byte> module)
    {
        using var stream = SlimMemoryStream.CreateReadOnly(module);

        return Unsafe.As<BridgeModule>(
            Activator.CreateInstance(
                new BridgeModuleAssemblyLoadContext()
                    .LoadFromStream(stream)
                    .DefinedTypes
                    .Single(static type => type.BaseType == typeof(BridgeModule)))!);
    }
}
