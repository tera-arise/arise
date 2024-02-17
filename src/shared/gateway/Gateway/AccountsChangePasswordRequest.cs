using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsChangePasswordRequest
{
    [Password]
    public required string Password { get; init; }
}
