using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsVerifyEmailChangeRequest
{
    [Token]
    public required string Token { get; init; }
}
