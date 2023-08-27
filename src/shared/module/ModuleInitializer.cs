using Arise.Bridge.Protection;

namespace Arise.Bridge;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    [SuppressMessage("", "CA2255")]
    public static void Initialize()
    {
        GameProtection.Initialize();
    }
}
