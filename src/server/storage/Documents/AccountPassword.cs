namespace Arise.Server.Storage.Documents;

public sealed class AccountPassword
{
    public required AccountPasswordKind Kind { get; init; }

    public required byte[] Salt { get; init; }

    public required byte[] Hash { get; init; }
}
