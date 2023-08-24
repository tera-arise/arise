namespace Arise.Server.Web.Models.Api.Accounts;

internal sealed class AccountsRecoverRequest
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string Address { get; init; }
}
