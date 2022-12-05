namespace Arise.Server.Storage.Documents;

public sealed class AccountBan
{
    public required Interval Interval { get; set; }

    public string? Reason { get; set; }
}
