using Arise.Server.Web.DataAnnotations;

namespace Arise.Server.Web.Models.Api.Accounts;

public sealed class AccountsCreateResponse
{
    [Token]
    public required string? GameKey { get; init; }
}
