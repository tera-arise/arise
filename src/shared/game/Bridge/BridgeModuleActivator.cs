namespace Arise.Bridge;

public static class BridgeModuleActivator
{
    [UnconditionalSuppressMessage("", "IL2026")]
    [UnconditionalSuppressMessage("", "IL2072")]
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
