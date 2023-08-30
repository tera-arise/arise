using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsUpdateRequest
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string? Email { get; init; }

    [Password]
    public required string? Password { get; init; }
}
