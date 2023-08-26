using Arise.Server.Web.DataAnnotations;

namespace Arise.Server.Web.Models.Api.Accounts;

internal sealed class AccountsCreateResponse
{
    [Token]
    public required string? SessionTicket { get; init; }
}
