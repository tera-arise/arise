namespace Arise.Client.Symbiote.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x10004)]
public unsafe struct S1ClientSocketBuffer
{
    [InlineArray(0x10000)]
    public struct InlineArray_data
    {
        private byte _element;
    }

    public static delegate* unmanaged<S1ClientSocketBuffer*, int, int> Consume { get; } =
        (delegate* unmanaged<S1ClientSocketBuffer*, int, int>)TERA.Resolve(0x7ff69babc170);

    public static delegate* unmanaged<S1ClientSocketBuffer*, int> GetPosition { get; } =
        (delegate* unmanaged<S1ClientSocketBuffer*, int>)TERA.Resolve(0x7ff69bab21f0);

    public static delegate* unmanaged<S1ClientSocketBuffer*, byte*, int, BOOL> Write { get; } =
        (delegate* unmanaged<S1ClientSocketBuffer*, byte*, int, BOOL>)TERA.Resolve(0x7ff69bacacd0);

    [FieldOffset(0x0)]
    public InlineArray_data data;

    [FieldOffset(0x10000)]
    public int position;
}
