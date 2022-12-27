using Arise.Server.Web.DataAnnotations;

namespace Arise.Server.Web.Models.Api.Accounts;

public sealed class AccountsCreateRequest
{
    [DataAnnotations.Email]
    public required string Address { get; init; }

    [Password]
    public required string Password { get; init; }
}
