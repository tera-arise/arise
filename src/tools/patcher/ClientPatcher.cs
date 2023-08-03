using static Iced.Intel.AssemblerRegisters;

namespace Arise.Tools.Patcher;

internal static class ClientPatcher
{
    public static async Task PatchAsync(PatcherOptions options)
    {
        await Terminal.OutLineAsync($"Loading PE '{options.Executable}'...");

        await using var stream = options.Executable.Open(FileMode.Open);

        var pe = new PeFile(stream);
        var imageBase = pe.ImageNtHeaders!.OptionalHeader.ImageBase;
        var textSection = pe.ImageSectionHeaders![0];

        var writer = new StreamCodeWriter(stream);

        async ValueTask PatchAsync(string name, ulong address, Action<Assembler> assemble)
        {
            var rva = address - imageBase;
            var fp = textSection.PointerToRawData + (rva - textSection.VirtualAddress);

            await Terminal.OutLineAsync($"Patching '{name}' at VA: 0x{address:x} RVA: 0x{rva:x} FP: 0x{fp:x}...");

            stream.Position = (long)fp;

            var asm = new Assembler(64);

            assemble(asm);

            // TODO: What should RIP be here? (Unimportant for our current patches.)
            _ = asm.Assemble(writer, address);
        }

        // This just appears to be a fairly standard routine that performs a memory dump and sends a crash report. It
        // does not seem to actually be used in retail builds of TERA, but patch it just in case.
        await PatchAsync(
            "S1CrashDump::SendDump",
            0x7ff69a947550,
            asm =>
            {
                asm.xor(eax, eax);
                asm.ret();
            });

        // Unsure if this one is for diagnostic or cheat detection purposes. Either way, it sends a bunch of system
        // details without user consent, so patch it.
        await PatchAsync(
            "S1LagLogDataSendingThread::SendReport",
            0x7ff69b78e860,
            asm =>
            {
                asm.ret();
            });

        // This one is presumably used to detect private servers. It's a bit sneaky; the server details are
        // (sometimes, randomly) sent in the query string of an otherwise empty HTTP request to a static IP address,
        // tipping the developers off. Definitely get rid of this one.
        await PatchAsync(
            "S1LobbySceneServer::SnoopLoginArbiter",
            0x7ff69bd409c0,
            asm =>
            {
                asm.ret();
            });

        await Terminal.OutLineAsync($"Saving PE '{options.Executable}'...");
    }
}
