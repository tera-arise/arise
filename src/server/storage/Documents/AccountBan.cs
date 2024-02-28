namespace Arise.Server.Storage.Documents;

public sealed class AccountBan
{
    public required DateTime Expiry { get; set; }

    public required string Reason { get; set; }
}
