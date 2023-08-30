using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsCreateResponse
{
    [Token]
    public required string SessionTicket { get; init; }
}
