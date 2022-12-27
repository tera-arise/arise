namespace Arise.Server.Web.Models.Api.Accounts;

public sealed class AccountsRecoverRequest
{
    [DataAnnotations.Email]
    public required string Address { get; init; }
}
