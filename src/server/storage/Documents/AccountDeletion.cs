namespace Arise.Server.Storage.Documents;

public sealed class AccountDeletion
{
    public required Instant Due { get; init; }

    public required AccountToken? Verification { get; set; }
}
