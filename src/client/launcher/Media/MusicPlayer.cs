namespace Arise.Client.Launcher.Media;

[RegisterSingleton<MusicPlayer>]
internal sealed class MusicPlayer : IDisposable
{
    private static readonly ReadOnlyMemory<ReadOnlyMemory<byte>> _assets =
        AssetLoader
            .GetAssets(new($"avares://{ThisAssembly.AssemblyName}/assets"), null)
            .Where(static uri => Path.GetFileName(uri.AbsolutePath).StartsWith("music_", StringComparison.Ordinal))
            .Select(static uri =>
            {
                using var stream = AssetLoader.Open(uri);

                var buffer = GC.AllocateUninitializedArray<byte>((int)stream.Length);

                stream.ReadExactly(buffer);

                return (ReadOnlyMemory<byte>)buffer;
            })
            .ToArray();

    private readonly WasapiOut _wasapi = new();

    void IDisposable.Dispose()
    {
        _wasapi.Dispose();
    }

    [SuppressMessage("", "CA2000")]
    [SuppressMessage("", "CA5394")]
    public void PlayRandom()
    {
        _wasapi.Init(
            new LoopStream(
                new WaveChannel32(
                    new VorbisWaveReader(
                        new SlimMemoryStream
                        {
                            Buffer = MemoryMarshal.AsMemory(_assets.Span[Random.Shared.Next() % _assets.Length]),
                        }),
                    volume: 0.1f,
                    pan: 0.0f)));

        _wasapi.Play();
    }

    public void Stop()
    {
        _wasapi.Stop();
    }
}
