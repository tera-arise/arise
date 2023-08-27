internal static partial class ThisAssembly
{
    public static string GameTitle { get; } = typeof(ThisAssembly).Assembly.GetMetadata("Arise.GameTitle");
}
