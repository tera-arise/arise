namespace Arise.Client.Launcher.Media;

[RegisterSingleton<MusicPlayer>]
internal sealed class MusicPlayer : IDisposable
{
    private readonly WasapiOut _wasapi = new();

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
