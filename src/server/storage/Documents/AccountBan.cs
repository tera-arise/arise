namespace Arise.Server.Storage.Documents;

public sealed class AccountBan
{
    public required Instant Expiry { get; set; }

    public required string Reason { get; set; }
}
