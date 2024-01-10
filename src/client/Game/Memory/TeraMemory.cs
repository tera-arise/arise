namespace Arise.Client.Game.Memory;

internal static unsafe class TeraMemory
{
    // Note: This class is only safe to use after the game has been woken up from suspension.

    public static T* Alloc<T>()
        where T : unmanaged
    {
        return (T*)S1.appMalloc((uint)sizeof(T), (uint)sizeof(void*));
    }

    public static T* AllocArray<T>(int length)
        where T : unmanaged
    {
        return (T*)S1.appMalloc((uint)(sizeof(T) * length), (uint)sizeof(void*));
    }

    public static T* ReallocArray<T>(void* ptr, int length)
        where T : unmanaged
    {
        return (T*)S1.appRealloc(ptr, (uint)(sizeof(T) * length), (uint)sizeof(void*));
    }

    public static void Free<T>(T* ptr)
        where T : unmanaged
    {
        S1.appFree(ptr);
    }
}
