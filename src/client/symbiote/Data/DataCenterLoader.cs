using Arise.Client.Memory;
using Arise.Client.Net;
using Vezel.Novadrop.Interop;
using static Iced.Intel.AssemblerRegisters;

namespace Arise.Client.Data;

[RegisterSingleton<DataCenterLoader>]
[SuppressMessage("", "CA1812")]
internal sealed class DataCenterLoader : IHostedService
{
    private readonly BridgeDataComponent _dataComponent;

    public DataCenterLoader(GameClient client)
    {
        _dataComponent = client.Session.Module.Data;
    }

    unsafe Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        // Patch in the key and IV.
        DynamicAssembler.Patch(
            (void*)0x7ff69b884af4,
            asm =>
            {
                static uint ReadUInt32(ReadOnlySpan<byte> buffer, int index)
                {
                    return MemoryMarshal.Read<uint>(buffer[(sizeof(uint) * index)..]);
                }

                var key = _dataComponent.Key.Span;

                asm.mov(__dword_ptr[r11 - 0x48], ReadUInt32(key, 0));
                asm.mov(__dword_ptr[r11 - 0x44], ReadUInt32(key, 1));
                asm.mov(__dword_ptr[r11 - 0x40], ReadUInt32(key, 2));
                asm.mov(__dword_ptr[r11 - 0x3c], ReadUInt32(key, 3));

                var iv = _dataComponent.IV.Span;

                asm.mov(__dword_ptr[r11 - 0x58], ReadUInt32(iv, 0));
                asm.mov(__dword_ptr[r11 - 0x54], ReadUInt32(iv, 1));
                asm.mov(__dword_ptr[r11 - 0x50], ReadUInt32(iv, 2));
                asm.mov(__dword_ptr[r11 - 0x4c], ReadUInt32(iv, 3));
            });

        using var stream = EmbeddedDataCenter.OpenStream();

        var length = (int)stream.Length;
        var buffer = TeraMemory.AllocArray<byte>(length);
        var reader = TeraMemory.Alloc<FBufferReader>();

        _ = FBufferReader.__ctor(reader, buffer, length, true, false);

        stream.ReadExactly(new(buffer, length));

        // Patch in the FBufferReader pointer. S1DataDB::UnpackArchive() will delete the reader and its buffer.
        DynamicAssembler.Patch((void*)0x7ff69bb19f69, asm => asm.mov(rax, (ulong)reader));

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
