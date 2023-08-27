namespace Arise.Client.Memory;

internal static class DynamicAssembler
{
    public static unsafe void Patch(void* ptr, Action<Assembler> assemble)
    {
        var asm = new Assembler(64);

        assemble(asm);

        _ = asm.Assemble(new RawCodeWriter((byte*)ptr), (ulong)ptr);
    }
}
