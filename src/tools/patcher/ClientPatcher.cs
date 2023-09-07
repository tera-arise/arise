using static Iced.Intel.AssemblerRegisters;

namespace Arise.Tools.Patcher;

internal static class ClientPatcher
{
    public static async ValueTask PatchAsync(PatcherOptions options)
    {
        await Terminal.OutLineAsync($"Loading PE '{options.OriginalTeraExecutableFile}'...");

        var bytes = await File.ReadAllBytesAsync(options.OriginalTeraExecutableFile.FullName);

        await using var stream = new MemoryStream(bytes, 0, bytes.Length, writable: true, publiclyVisible: true);

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

        // These functions are used to guard various functionality intended for developers based on the domain name of
        // the computer running the game. Ensure that they always return false regardless of network configuration.
        foreach (var (name, addr) in new[] { ("isCompanyDomain", 0x7ff69a944290u), ("isDevDomain", 0x7ff69a9444e0u) })
            await PatchAsync(
                name,
                addr,
                static asm =>
                {
                    asm.xor(eax, eax);
                    asm.ret();
                });

        // This just appears to be a fairly standard routine that performs a memory dump and sends a crash report. It
        // does not seem to actually be used in retail builds of TERA, but patch it just in case.
        await PatchAsync(
            "S1CrashDump::SendDump",
            0x7ff69a947550,
            static asm =>
            {
                asm.xor(eax, eax);
                asm.ret();
            });

        // Packet checksums were added to the game as a security measure. However, the algorithm is public knowledge by
        // now and so offers no meaningful security. Patch out the wasteful computation and just return the seed.
        await PatchAsync(
            "S1ConnectionManager::ComputeChecksum",
            0x7ff69b472da0,
            static asm =>
            {
                asm.mov(eax, r8d);
                asm.ret();
            });

        // Unsure if this one is for diagnostic or cheat detection purposes. Either way, it sends a bunch of system
        // details without user consent, so patch it.
        await PatchAsync("S1LagLogDataSendingThread::SendReport", 0x7ff69b78e860, static asm => asm.ret());

        // Disable launching the damage meter application and serving messages from the named pipe.
        await PatchAsync("S1TeraAddOnPipeBase::Connect", 0x7ff69b9444e0, static asm => asm.ret());
        await PatchAsync("S1TeraAddOnManager::Initialize", 0x7ff69b9459e0, static asm => asm.ret());

        // Disable scheduling S1AutoLogoutJob. We will handle idle disconnection on the server side.
        await PatchAsync("S1Context::Initialize", 0x7ff69bab64e7, static asm => asm.nop(57));

        // These are all hooked by the symbiote to integrate the QUIC-based network protocol. Make sure that the
        // unhooked versions of the functions just cause a crash.
        await PatchAsync("S1Connection::Connect", 0x7ff69baa9fc0, static asm => asm.ud2());
        await PatchAsync("S1Connection::Disconnect", 0x7ff69baaa530, static asm => asm.ud2());
        await PatchAsync("S1CommandQueue::RunCommands", 0x7ff69baaa560, static asm => asm.ud2());
        await PatchAsync("S1ConnectionManager::SendPacket", 0x7ff69babe7b0, static asm => asm.ud2());

        // The symbiote will hook this function and provide a memory-backed FArchive. Make sure that the unhooked
        // version of the function just causes a crash.
        await PatchAsync("S1DataDB::Initialize", 0x7ff69bb19f30, static asm => asm.ud2());
        await PatchAsync("S1DataDB::Initialize", 0x7ff69bb19f69, static asm => asm.nop(23));

        // This one is presumably used to detect private servers. It's a bit sneaky; the server details are
        // (sometimes, randomly) sent in the query string of an otherwise empty HTTP request to a static IP address,
        // tipping the developers off. Definitely get rid of this one.
        await PatchAsync("S1LobbySceneServer::SnoopLoginArbiter", 0x7ff69bd409c0, static asm => asm.ret());

        await Terminal.OutLineAsync($"Saving PE '{options.PatchedTeraExecutableFile}'...");

        await File.WriteAllBytesAsync(options.PatchedTeraExecutableFile.FullName, stream.GetBuffer());
    }
}
