// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Launcher.Media;

namespace Arise.Client.Launcher.Controllers;

[SuppressMessage("", "CA1515")]
public sealed class MainController : LauncherController
{
    private readonly MusicPlayer _musicPlayer;

    public MainController(IServiceProvider services, MusicPlayer musicPlayer)
        : base(services)
    {
        _musicPlayer = musicPlayer;
    }

    public void PlayMusic()
    {
        _musicPlayer.Play();
    }

    public void StopMusic()
    {
        _musicPlayer.Stop();
    }
}
