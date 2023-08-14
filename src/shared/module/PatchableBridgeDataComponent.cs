namespace Arise.Bridge;

public sealed class PatchableBridgeDataComponent : BridgeDataComponent
{
    [Obfuscation]
    protected override void InitializeKey(Span<byte> key)
    {
        // Filled in by the server's BridgeModuleProvider.

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
    protected override void InitializeIV(Span<byte> iv)
    {
        // Filled in by the server's BridgeModuleProvider.

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
