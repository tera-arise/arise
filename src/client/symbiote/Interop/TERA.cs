namespace Arise.Client.Symbiote.Interop;

public static unsafe class TERA
{
    public delegate void* Resolver(ulong address);

    private static readonly object _lock = new();

    private static Resolver _resolver = addr => (void*)addr;

    public static void SetResolver(Resolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        lock (_lock)
            _resolver = resolver;
    }

    internal static void* Resolve(ulong address)
    {
        lock (_lock)
            return _resolver(address);
    }
}
