internal static partial class ThisAssembly
{
    public static string GameTitle { get; }

    public static int GameRevision { get; }

    static ThisAssembly()
    {
        var asm = typeof(ThisAssembly).Assembly;

        GameTitle = asm.GetMetadata("Arise.GameTitle");
        GameRevision = int.Parse(asm.GetMetadata("Arise.GameRevision"), CultureInfo.InvariantCulture);
    }
}
