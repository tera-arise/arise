namespace Arise.Gateway;

public sealed class AccountsChangeEmailRequest
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string Email { get; init; }
}
