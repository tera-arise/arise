namespace Arise.Client.Symbiote.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x64)]
public unsafe struct S1CommandQueue
{
    public static delegate* unmanaged<S1CommandQueue*, byte*, void> EnqueuePacket { get; } =
        (delegate* unmanaged<S1CommandQueue*, byte*, void>)TERA.Resolve(0x7ff69baac250);

    public static delegate* unmanaged<S1CommandQueue*, void> RunCommands { get; } =
        (delegate* unmanaged<S1CommandQueue*, void>)TERA.Resolve(0x7ff69baaa560);

    public static delegate* unmanaged<S1CommandQueue*, byte*, BOOL> RunPacketHandler { get; } =
        (delegate* unmanaged<S1CommandQueue*, byte*, BOOL>)TERA.Resolve(0x7ff69baad410);
}
