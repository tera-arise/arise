namespace Arise.Module;

internal static unsafe class ModuleInitializer
{
    [ModuleInitializer]
    [SuppressMessage("", "CA2255")]
    public static void Initialize()
    {
        GameProtection.Initialize();
    }
}
