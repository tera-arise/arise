namespace Arise.Server.Storage.Documents;

public sealed class AccountBan
{
    public required Interval Period { get; set; }

    public required string Reason { get; set; }
}
