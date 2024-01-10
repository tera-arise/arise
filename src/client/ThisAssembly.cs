internal static partial class ThisAssembly
{
    public static string GameTitle { get; }

    public static (Uri Gateway, Uri World)? ServerUris { get; }

    static ThisAssembly()
    {
        var asm = typeof(ThisAssembly).Assembly;

        GameTitle = asm.GetMetadata("Arise.GameTitle");

        if (asm.TryGetMetadata("Arise.GatewayServerUri", out var gateway) &&
            asm.TryGetMetadata("Arise.WorldServerUri", out var world))
            ServerUris = (new(gateway), new(world));
    }
}
