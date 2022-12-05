namespace Arise.IO;

public class GameBinaryWriter : StreamBinaryWriter
{
    public GameBinaryWriter(Stream stream)
        : base(stream)
    {
    }

    public void WriteVector3(Vector3 value)
    {
        WriteSingle(value.X);
        WriteSingle(value.Y);
        WriteSingle(value.Z);
    }

    public async ValueTask WriteVector3Async(Vector3 value, CancellationToken cancellationToken = default)
    {
        await WriteSingleAsync(value.X, cancellationToken).ConfigureAwait(false);
        await WriteSingleAsync(value.Y, cancellationToken).ConfigureAwait(false);
        await WriteSingleAsync(value.Z, cancellationToken).ConfigureAwait(false);
    }
}
