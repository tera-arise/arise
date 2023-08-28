using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsAuthenticateResponse
{
    public required bool IsVerifying { get; init; }

    public required bool IsChangingEmail { get; init; }

    public required bool IsRecovered { get; init; }

    public required bool IsDeleting { get; init; }

    public required string? BanReason { get; init; }

    [Token]
    public required string? SessionTicket { get; init; }
}
