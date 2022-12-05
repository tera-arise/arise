namespace Arise.Modules;

public static class BridgeModuleActivator
{
    [UnconditionalSuppressMessage("", "IL2026")]
    [UnconditionalSuppressMessage("", "IL2070")]
    [UnconditionalSuppressMessage("", "IL2072")]
    public static BridgeModule Create(Assembly assembly)
    {
        return (BridgeModule)Activator.CreateInstance(
            assembly.DefinedTypes.Single(t => t.ImplementedInterfaces.Contains(typeof(BridgeModule))))!;
    }
}
