using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsCreateRequest
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string Address { get; init; }

    [Password]
    public required string Password { get; init; }
}
