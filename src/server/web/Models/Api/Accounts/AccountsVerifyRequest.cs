using Arise.Server.Web.DataAnnotations;

namespace Arise.Server.Web.Models.Api.Accounts;

public sealed class AccountsVerifyRequest
{
    [Token]
    public required string Token { get; init; }
}
