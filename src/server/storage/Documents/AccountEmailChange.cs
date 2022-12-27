namespace Arise.Server.Storage.Documents;

public sealed class AccountEmailChange
{
    public required string Address { get; init; }

    public required AccountToken Verification { get; init; }
}
