// SPDX-License-Identifier: AGPL-3.0-or-later

internal static partial class ThisAssembly
{
    public static ReadOnlyMemory<byte> DataCenterKey { get; }

    public static ReadOnlyMemory<byte> DataCenterIV { get; }

    static ThisAssembly()
    {
        var asm = typeof(ThisAssembly).Assembly;

        DataCenterKey = Convert.FromHexString(asm.GetMetadata("Arise.DataCenterKey"));
        DataCenterIV = Convert.FromHexString(asm.GetMetadata("Arise.DataCenterIV"));
    }
}
