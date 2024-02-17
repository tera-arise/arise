namespace Arise.Server.Storage.Documents;

public sealed class AccountToken
{
    public required string Value { get; init; }

    public required Instant Expiry { get; init; }
}
