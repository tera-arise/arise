namespace Arise.Client.Launcher.Controllers;

[RegisterTransient<MainController>]
internal sealed class MainController : LauncherController
{
    public Bitmap Background => _background.Value;

    [SuppressMessage("", "CA5394")]
    private readonly Lazy<Bitmap> _background = new(
        static () =>
        {
            var candidates = AssetLoader
                .GetAssets(new($"avares://{ThisAssembly.AssemblyName}/assets"), null)
                .Where(static uri =>
                    Path.GetFileName(uri.AbsolutePath).StartsWith("background_", StringComparison.Ordinal))
                .ToArray();

            using var stream = AssetLoader.Open(candidates[Random.Shared.Next() % candidates.Length]);

            return new(stream);
        },
        isThreadSafe: false);

    public MainController(IServiceProvider services)
        : base(services)
    {
    }
}
