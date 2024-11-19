// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Launcher.Media;

[SuppressMessage("", "CA1515")]
public sealed class MusicPlayer : IDisposable
{
    private readonly WasapiOut _wasapi = new();

    [SuppressMessage("", "CA1063")]
    void IDisposable.Dispose()
    {
        _wasapi.Dispose();
    }

    [SuppressMessage("", "CA2000")]
    public void Play()
    {
        _wasapi.Init(
            new LoopStream(
                new WaveChannel32(
                    new VorbisWaveReader(EmbeddedMediaAssets.Open("audio_launcher.ogg")), volume: 0.1f, pan: 0.0f)));

        _wasapi.Play();
    }

    public void Stop()
    {
        _wasapi.Stop();
    }
}
