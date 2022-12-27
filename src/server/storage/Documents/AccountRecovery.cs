namespace Arise.Server.Storage.Documents;

public sealed class AccountRecovery
{
    public required AccountPassword Password { get; init; }

    public required Interval Period { get; init; }
}
