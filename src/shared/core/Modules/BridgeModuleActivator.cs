namespace Arise.Modules;

public static class BridgeModuleActivator
{
    public static BridgeModule Create(Assembly assembly)
    {
        return (BridgeModule)Activator.CreateInstance(
            assembly.DefinedTypes.Single(t => t.ImplementedInterfaces.Contains(typeof(BridgeModule))))!;
    }
}
