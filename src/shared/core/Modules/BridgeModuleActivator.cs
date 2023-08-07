namespace Arise.Modules;

public static class BridgeModuleActivator
{
    public static BridgeModule Create(Assembly assembly)
    {
        return Unsafe.As<BridgeModule>(
            Activator.CreateInstance(assembly.DefinedTypes.Single(static t => t.BaseType == typeof(BridgeModule)))!);
    }
}
