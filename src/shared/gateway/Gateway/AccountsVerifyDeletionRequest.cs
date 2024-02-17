using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsVerifyDeletionRequest
{
    [Token]
    public required string Token { get; init; }
}
