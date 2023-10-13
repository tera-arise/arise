using Arise.Client.Launcher.Media;

namespace Arise.Client.Launcher.Controllers;

[RegisterTransient<MainController>]
internal sealed class MainController : LauncherController
{
    public Bitmap Background { get; }

    public Bitmap Logo { get; }

    private readonly MusicPlayer _musicPlayer;

    public MainController(IServiceProvider services, MusicPlayer musicPlayer)
        : base(services)
    {
        Background = new(EmbeddedMediaAssets.Open("image_background.webp"));
        Logo = new(EmbeddedMediaAssets.Open("image_logo.webp"));
        _musicPlayer = musicPlayer;

        this.WhenActivated((CompositeDisposable disposable) =>
        {
            _musicPlayer.Play();

            disposable.Add(Disposable.Create(_musicPlayer, static player => player.Stop()));
        });
    }
}
