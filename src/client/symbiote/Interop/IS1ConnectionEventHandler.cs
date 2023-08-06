namespace Arise.Client.Symbiote.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct IS1ConnectionEventHandler
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public IS1MemoryObject.VirtualFunctionTable IS1MemoryObject;

        public delegate* unmanaged<IS1ConnectionEventHandler*, BOOL, void> OnConnect;

        public delegate* unmanaged<IS1ConnectionEventHandler*, void> OnDisconnect;

        public delegate* unmanaged<IS1ConnectionEventHandler*, byte*, int, void> OnReceive;
    }

    public static delegate* unmanaged<IS1ConnectionEventHandler*, byte*, int, void> OnReceive { get; } =
        (delegate* unmanaged<IS1ConnectionEventHandler*, byte*, int, void>)TERA.Resolve(0x7ff69b9c1ce0);

    [FieldOffset(0x0)]
    public IS1MemoryObject IS1MemoryObject;

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;
}
