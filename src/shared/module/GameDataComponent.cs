namespace Arise.Module;

public sealed class GameDataComponent : DataComponent
{
    public override ReadOnlyMemory<byte> Key { get; }

    public override ReadOnlyMemory<byte> IV { get; }

    public GameDataComponent()
    {
        var key = new byte[16];

        InitializeKey(key);

        Key = key;

        var iv = new byte[16];

        InitializeIV(iv);

        IV = iv;
    }

    [Obfuscation]
    private static void InitializeKey(Span<byte> key)
    {
        key[0] = 42;
        key[1] = 42;
        key[2] = 42;
        key[3] = 42;
        key[4] = 42;
        key[5] = 42;
        key[6] = 42;
        key[7] = 42;
        key[8] = 42;
        key[9] = 42;
        key[10] = 42;
        key[11] = 42;
        key[12] = 42;
        key[13] = 42;
        key[14] = 42;
        key[15] = 42;
    }

    [Obfuscation]
    private static void InitializeIV(Span<byte> iv)
    {
        iv[0] = 42;
        iv[1] = 42;
        iv[2] = 42;
        iv[3] = 42;
        iv[4] = 42;
        iv[5] = 42;
        iv[6] = 42;
        iv[7] = 42;
        iv[8] = 42;
        iv[9] = 42;
        iv[10] = 42;
        iv[11] = 42;
        iv[12] = 42;
        iv[13] = 42;
        iv[14] = 42;
        iv[15] = 42;
    }
}
