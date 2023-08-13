namespace Vezel.Novadrop.Interop;

public static unsafe class Tera
{
    public delegate void* Resolver(ulong address);

    private static volatile Resolver _resolver = static addr => (void*)addr;

    public static void SetResolver(Resolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _resolver = resolver;
    }

    internal static void* Resolve(ulong address)
    {
        return _resolver(address);
    }
}
