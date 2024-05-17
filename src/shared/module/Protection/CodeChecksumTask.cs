// SPDX-License-Identifier: AGPL-3.0-or-later

using static Arise.Bridge.WindowsPInvoke;

namespace Arise.Bridge.Protection;

internal sealed unsafe class CodeChecksumTask : GameProtectionTask
{
    private byte* _text;

    private int _textLength;

    private uint _textChecksum;

    protected override void Initialize()
    {
        var dosHdr = (IMAGE_DOS_HEADER*)RtlGetCurrentPeb()->ImageBaseAddress;
        var ntHdr = (IMAGE_NT_HEADERS*)((byte*)dosHdr + dosHdr->e_lfanew);
        var sctHdr = (IMAGE_SECTION_HEADER*)((byte*)&ntHdr->OptionalHeader + ntHdr->FileHeader.SizeOfOptionalHeader);

        // We assume that the .text section is the first section. This assumption would only be false if someone has
        // tampered with the executable. Doing so would break Themida-virtualized code (e.g. S1Context constructor), or
        // produce an executable that cannot even be loaded, so this is not much of a concern.
        _text = (byte*)dosHdr + sctHdr->VirtualAddress;
        _textLength = (int)sctHdr->VirtualSize;
        _textChecksum = CalculateChecksum();
    }

    protected override bool Check()
    {
        return CalculateChecksum() == _textChecksum;
    }

    private uint CalculateChecksum()
    {
        var checksum = 0u;
        var i = 0;

        for (; i < _textLength - _textLength % sizeof(ulong); i += sizeof(ulong))
            checksum = BitOperations.Crc32C(checksum, Unsafe.ReadUnaligned<ulong>(_text + i));

        for (; i < _textLength; i++)
            checksum = BitOperations.Crc32C(checksum, _text[i]);

        return checksum;
    }
}
