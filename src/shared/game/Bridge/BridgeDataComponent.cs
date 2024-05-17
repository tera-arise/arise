// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Bridge;

public abstract class BridgeDataComponent
{
    public ReadOnlyMemory<byte> Key { get; }

    public ReadOnlyMemory<byte> IV { get; }

    [SuppressMessage("", "CA2214")]
    protected BridgeDataComponent()
    {
        var key = GC.AllocateUninitializedArray<byte>(16);
        var iv = GC.AllocateUninitializedArray<byte>(16);

        InitializeKey(key);
        InitializeIV(iv);

        Key = key;
        IV = iv;
    }

    protected abstract void InitializeKey(scoped Span<byte> key);

    protected abstract void InitializeIV(scoped Span<byte> iv);
}
