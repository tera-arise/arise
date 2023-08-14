namespace Arise.Server.Data;

internal static class DataCenterParameters
{
    public static ReadOnlyMemory<byte> Key { get; }

    public static ReadOnlyMemory<byte> IV { get; }

    static DataCenterParameters()
    {
        var attrs = typeof(ThisAssembly).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

        byte[] GetByteArray(string key)
        {
            return Convert.FromHexString(attrs.Single(attr => attr.Key == $"Arise.DataCenter{key}").Value!);
        }

        Key = GetByteArray("Key");
        IV = GetByteArray("IV");
    }
}
