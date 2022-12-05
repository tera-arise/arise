namespace Arise.IO;

public class GameBinaryReader : StreamBinaryReader
{
    public GameBinaryReader(Stream stream)
        : base(stream)
    {
    }

    public Vector3 ReadVector3()
    {
        return new(ReadSingle(), ReadSingle(), ReadSingle());
    }

    public async ValueTask<Vector3> ReadVector3Async(CancellationToken cancellationToken = default)
    {
        return new(
            await ReadSingleAsync(cancellationToken).ConfigureAwait(false),
            await ReadSingleAsync(cancellationToken).ConfigureAwait(false),
            await ReadSingleAsync(cancellationToken).ConfigureAwait(false));
    }
}
