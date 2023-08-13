namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct IS1ConnectionInfo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public IS1MemoryObject.VirtualFunctionTable IS1MemoryObject;

        public delegate* unmanaged<IS1ConnectionInfo*, void> method_002;

        public delegate* unmanaged<IS1ConnectionInfo*, void> method_003;

        public delegate* unmanaged<IS1ConnectionInfo*, S1ClientSocket*> CreateSocket;
    }

    [FieldOffset(0x0)]
    public IS1MemoryObject IS1MemoryObject;

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;
}
