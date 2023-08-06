namespace Arise.Client.Symbiote.Interop;

public static unsafe class S1
{
    public static ref S1Context* Context => ref *(S1Context**)TERA.Resolve(0x7ff69d7b2040);
}
