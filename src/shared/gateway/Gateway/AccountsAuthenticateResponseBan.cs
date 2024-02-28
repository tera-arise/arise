namespace Arise.Gateway;

public sealed class AccountsAuthenticateResponseBan
{
    public required DateTime Expiry { get; init; }

    public required string? Reason { get; init; }
}
