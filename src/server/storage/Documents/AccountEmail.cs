namespace Arise.Server.Storage.Documents;

public sealed class AccountEmail
{
    public required string Address { get; init; }

    public required AccountToken? Verification { get; set; }
}
