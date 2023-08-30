namespace Arise.Gateway;

public sealed class AccountsRecoverRequest
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string Email { get; init; }
}
