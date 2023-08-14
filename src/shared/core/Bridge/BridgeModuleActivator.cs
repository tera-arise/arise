namespace Arise.Bridge;

public static class BridgeModuleActivator
{
    public static BridgeModule Activate(ReadOnlyMemory<byte> module)
    {
        using var stream = new SlimMemoryStream
        {
            Buffer = MemoryMarshal.AsMemory(module),
        };

        var context = new BridgeModuleAssemblyLoadContext();
        var obj = Unsafe.As<BridgeModule>(
            Activator.CreateInstance(
                context
                    .LoadFromStream(stream)
                    .DefinedTypes
                    .Single(static type => type.BaseType == typeof(BridgeModule)))!);

        // Make the assembly available for collection when the BridgeModule instance is no longer referenced.
        context.Unload();

        return obj;
    }
}
