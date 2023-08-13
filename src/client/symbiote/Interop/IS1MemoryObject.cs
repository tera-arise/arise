namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct IS1MemoryObject
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public delegate* unmanaged<IS1MemoryObject*, uint, IS1MemoryObject*> __dtor;

        public delegate* unmanaged<IS1MemoryObject*, int> GetSize;
    }

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;
}
