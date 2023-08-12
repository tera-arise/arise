namespace Arise.Modules;

public abstract class DataComponent
{
    public ReadOnlyMemory<byte> Key { get; }

    public ReadOnlyMemory<byte> IV { get; }

    [SuppressMessage("", "CA2214")]
    protected DataComponent()
    {
        var key = new byte[16];
        var iv = new byte[16];

        InitializeKey(key);
        InitializeIV(iv);

        Key = key;
        IV = iv;
    }

    protected abstract void InitializeKey(Span<byte> key);

    protected abstract void InitializeIV(Span<byte> iv);
}
