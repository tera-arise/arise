using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsVerifyRequest
{
    [Token]
    public required string Token { get; init; }
}
