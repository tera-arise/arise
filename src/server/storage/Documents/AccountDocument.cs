namespace Arise.Server.Storage.Documents;

public sealed class AccountDocument : IDocument
{
    [Identity]
    public required string Address { get; set; }

    public required string Salt { get; set; }

    public required string Verifier { get; set; }

    public required AccountAccess Access { get; set; }

    public AccountBan? Ban { get; set; }
}
