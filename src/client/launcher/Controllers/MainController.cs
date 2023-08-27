using Arise.Client.Launcher.Media;

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
                .GetAssets(new($"avares://{ThisAssembly.AssemblyName}"), null)
                .Where(static uri =>
                    Path.GetFileName(uri.AbsolutePath).StartsWith("background_", StringComparison.Ordinal))
                .ToArray();

            using var stream = AssetLoader.Open(candidates[Random.Shared.Next() % candidates.Length]);

            return new(stream);
        },
        isThreadSafe: false);

    private readonly MusicPlayer _musicPlayer;

    public MainController(IServiceProvider services, MusicPlayer musicPlayer)
        : base(services)
    {
        _musicPlayer = musicPlayer;

        this.WhenActivated((CompositeDisposable disposable) =>
        {
            _musicPlayer.PlayRandom();

            disposable.Add(Disposable.Create(_musicPlayer, static player => player.Stop()));
        });
    }
}
