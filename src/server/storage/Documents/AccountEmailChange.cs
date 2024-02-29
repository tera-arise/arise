namespace Arise.Server.Storage.Documents;

public sealed class AccountEmailChange
{
    public required string OriginalAddress { get; init; }

    public required AccountToken Verification { get; init; }
}
