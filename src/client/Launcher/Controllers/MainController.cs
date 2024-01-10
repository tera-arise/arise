using Arise.Client.Launcher.Media;

namespace Arise.Client.Launcher.Controllers;

public sealed class MainController : LauncherController
{
    public MainController(IServiceProvider services, MusicPlayer musicPlayer)
        : base(services)
    {
        this.WhenActivated((CompositeDisposable disposable) =>
        {
            musicPlayer.Play();

            disposable.Add(Disposable.Create(musicPlayer, static player => player.Stop()));
        });
    }
}
