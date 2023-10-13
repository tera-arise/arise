namespace Arise.Client.Launcher.Media;

internal static class EmbeddedMediaAssets
{
    public static Stream Open(string name)
    {
        return AssetLoader.Open(new($"avares://{ThisAssembly.AssemblyName}/{name}"));
    }
}
