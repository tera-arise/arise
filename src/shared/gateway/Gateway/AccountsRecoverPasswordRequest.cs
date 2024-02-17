namespace Arise.Gateway;

public sealed class AccountsRecoverPasswordRequest
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string Email { get; init; }
}
