namespace Arise.Client.Symbiote.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x2002c)]
public unsafe struct S1ClientSocket
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public IS1MemoryObject.VirtualFunctionTable IS1MemoryObject;

        public delegate* unmanaged<S1ClientSocket*, byte*, int, void> Send;

        public delegate* unmanaged<S1ClientSocket*, void> Register;

        public delegate* unmanaged<S1ClientSocket*, byte*, int, void> Receive;

        public delegate* unmanaged<S1ClientSocket*, void> Disconnect;
    }

    public static delegate* unmanaged<S1ClientSocket*, uint, ushort, BOOL> Connect { get; } =
        (delegate* unmanaged<S1ClientSocket*, uint, ushort, BOOL>)TERA.Resolve(0x7ff69baa9de0);

    public static delegate* unmanaged<S1ClientSocket*, void> Disconnect { get; } =
        (delegate* unmanaged<S1ClientSocket*, void>)TERA.Resolve(0x7ff69bab89f0);

    public static delegate* unmanaged<S1ClientSocket*, void> Dispose { get; } =
        (delegate* unmanaged<S1ClientSocket*, void>)TERA.Resolve(0x7ff69baaa4d0);

    public static delegate* unmanaged<S1ClientSocket*, void> FlushSend { get; } =
        (delegate* unmanaged<S1ClientSocket*, void>)TERA.Resolve(0x7ff69babe860);

    public static delegate* unmanaged<S1ClientSocket*, void> ProcessEvents { get; } =
        (delegate* unmanaged<S1ClientSocket*, void>)TERA.Resolve(0x7ff69babbfe0);

    public static delegate* unmanaged<S1ClientSocket*, byte*, int, void> Receive { get; } =
        (delegate* unmanaged<S1ClientSocket*, byte*, int, void>)TERA.Resolve(0x7ff69baba0f0);

    public static delegate* unmanaged<S1ClientSocket*, void> Register { get; } =
        (delegate* unmanaged<S1ClientSocket*, void>)TERA.Resolve(0x7ff69bab8ef0);

    public static delegate* unmanaged<S1ClientSocket*, byte*, int, void> Send { get; } =
        (delegate* unmanaged<S1ClientSocket*, byte*, int, void>)TERA.Resolve(0x7ff69babe6b0);

    [FieldOffset(0x0)]
    public IS1MemoryObject IS1MemoryObject;

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;

    [FieldOffset(0x8)]
    public SOCKET socket;

    [FieldOffset(0x10)]
    public WSAEVENT @event;

    [FieldOffset(0x18)]
    public BOOL connected;

    [FieldOffset(0x1c)]
    public S1ClientSocketBuffer receive_buffer;

    [FieldOffset(0x10020)]
    public S1ClientSocketBuffer send_buffer;

    [FieldOffset(0x20024)]
    public IS1ConnectionEventHandler* handler;
}