namespace Arise.Gateway;

public sealed class AccountsVerifyEmailChangeResponse
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string Email { get; init; }
}
