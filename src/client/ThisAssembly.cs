// SPDX-License-Identifier: AGPL-3.0-or-later

internal static partial class ThisAssembly
{
    public static string GameTitle { get; }

    public static (Uri Gateway, Uri Game)? ServerUris { get; }

    static ThisAssembly()
    {
        var asm = typeof(ThisAssembly).Assembly;

        GameTitle = asm.GetMetadata("Arise.GameTitle");

        if (asm.TryGetMetadata("Arise.GatewayServerUri", out var gateway) &&
            asm.TryGetMetadata("Arise.GameServerUri", out var game))
            ServerUris = (new(gateway), new(game));
    }
}
