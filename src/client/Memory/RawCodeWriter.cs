namespace Arise.Client.Memory;

internal sealed unsafe class RawCodeWriter : CodeWriter
{
    private byte* _address;

    public RawCodeWriter(byte* address)
    {
        _address = address;
    }

    public override void WriteByte(byte value)
    {
        *_address++ = value;
    }
}
