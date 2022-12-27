namespace Arise.Server.Storage.Documents;

public sealed class AccountDeletion
{
    public required Interval Period { get; init; }

    public required AccountToken? Verification { get; set; }
}
