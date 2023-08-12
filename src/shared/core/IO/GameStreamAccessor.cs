using Arise.Entities;

namespace Arise.IO;

public class GameStreamAccessor : StreamAccessor
{
    public GameStreamAccessor(Stream stream)
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

    public EntityId ReadEntityId()
    {
        return EntityId.FromRaw(ReadInt64());
    }

    public async ValueTask<EntityId> ReadEntityIdAsync(CancellationToken cancellationToken = default)
    {
        return EntityId.FromRaw(await ReadInt64Async(cancellationToken).ConfigureAwait(false));
    }

    public void WriteEntityId(EntityId value)
    {
        WriteInt64(value.ToRaw());
    }

    public ValueTask WriteEntityIdAsync(EntityId value, CancellationToken cancellationToken = default)
    {
        return WriteInt64Async(value.ToRaw(), cancellationToken);
    }
}
