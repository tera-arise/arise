namespace Arise.Server.Storage.Documents;

public sealed class AccountDocument : IDocument, IVersioned
{
    public Guid Id { get; init; }

    public Guid Version { get; set; }

    public required AccountEmail Email { get; set; }

    public required AccountPassword Password { get; set; }

    public required AccountAccess Access { get; set; }

    public AccountToken? SessionTicket { get; set; }

    public AccountEmailChange? ChangingEmail { get; set; }

    public AccountRecovery? Recovery { get; set; }

    public AccountDeletion? Deletion { get; set; }

    public AccountBan? Ban { get; set; }

    public static void ConfigureMarten(DocumentMapping<AccountDocument> mapping)
    {
        // Inner property access cannot be expressed with attributes.
        mapping.Index(doc => doc.Email.Address, idx => idx.IsUnique = true);
    }
}
