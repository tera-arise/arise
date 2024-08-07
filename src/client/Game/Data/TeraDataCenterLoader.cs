// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Game.Memory;
using Arise.Client.Game.Net;
using Arise.Client.Memory;
using static Iced.Intel.AssemblerRegisters;

namespace Arise.Client.Game.Data;

internal sealed unsafe class TeraDataCenterLoader : IHostedService
{
    private readonly CodeManager _codeManager;

    private readonly BridgeDataComponent _dataComponent;

    private FunctionHook _hook = null!;

    private FBufferReader** _slot;

    public TeraDataCenterLoader(CodeManager codeManager, GameClient client)
    {
        _codeManager = codeManager;
        _dataComponent = client.Session.Module.Data;
    }

    unsafe Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _hook = FunctionHook.Create(
            _codeManager, S1DataDB.Initialize, (delegate* unmanaged<S1DataDB*, void>)&InitializeHook, this);

        _hook.IsActive = true;

        _slot = (FBufferReader**)NativeMemory.Alloc((uint)sizeof(FBufferReader*));

        // Patch in the key and IV.
        DynamicAssembler.Patch(
            (void*)0x7ff69b884af4,
            static (asm, @this) =>
            {
                static uint ReadUInt32(ReadOnlySpan<byte> buffer, int index)
                {
                    return MemoryMarshal.Read<uint>(buffer[(sizeof(uint) * index)..]);
                }

                var key = @this._dataComponent.Key.Span;

                asm.mov(__dword_ptr[r11 - 0x48], ReadUInt32(key, 0));
                asm.mov(__dword_ptr[r11 - 0x44], ReadUInt32(key, 1));
                asm.mov(__dword_ptr[r11 - 0x40], ReadUInt32(key, 2));
                asm.mov(__dword_ptr[r11 - 0x3c], ReadUInt32(key, 3));

                var iv = @this._dataComponent.IV.Span;

                asm.mov(__dword_ptr[r11 - 0x58], ReadUInt32(iv, 0));
                asm.mov(__dword_ptr[r11 - 0x54], ReadUInt32(iv, 1));
                asm.mov(__dword_ptr[r11 - 0x50], ReadUInt32(iv, 2));
                asm.mov(__dword_ptr[r11 - 0x4c], ReadUInt32(iv, 3));
            },
            this);

        // Patch in code to read the FBufferReader pointer from _slot.
        DynamicAssembler.Patch(
            (void*)0x7ff69bb19f69,
            static (asm, @this) =>
            {
                asm.mov(rax, (nuint)@this._slot);
                asm.mov(rax, __qword_ptr[rax]);
            },
            this);

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        NativeMemory.Free(_slot);

        _hook.Dispose();

        return Task.CompletedTask;
    }

    [UnmanagedCallersOnly]
    private static void InitializeHook(S1DataDB* @this)
    {
        using var stream = EmbeddedDataCenter.OpenStream();

        var length = (int)stream.Length;
        var buffer = TeraMemory.AllocArray<byte>(length);
        var reader = TeraMemory.Alloc<FBufferReader>();

        stream.ReadExactly(new(buffer, length));

        // S1DataDB::UnpackArchive() will free reader and buffer after decryption and decompression.
        *Unsafe.As<TeraDataCenterLoader>(FunctionHook.Current.State)._slot =
            FBufferReader.__ctor(reader, buffer, length, true, false);
    }
}
