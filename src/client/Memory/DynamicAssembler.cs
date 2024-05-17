// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Memory;

internal static class DynamicAssembler
{
    public static unsafe void Patch<T>(void* ptr, Action<Assembler, T> assemble, T state)
    {
        var asm = new Assembler(64);

        assemble(asm, state);

        _ = asm.Assemble(new RawCodeWriter((byte*)ptr), (ulong)ptr);
    }
}
