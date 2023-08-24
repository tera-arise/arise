namespace Arise.Server.Web.Authentication;

internal sealed class AccountClaimsPrincipal : ClaimsPrincipal
{
    public AccountDocument Document { get; }

    public bool IsRecovered { get; }

    private static readonly ReadOnlyMemory<(AccountAccess, Claim)> _claims =
        Enum
            .GetValues<AccountAccess>()
            .Order()
            .Select(static access => (access, new Claim(ClaimTypes.Role, access.ToString())))
            .ToArray();

    public AccountClaimsPrincipal(AccountDocument account, bool recovered)
    {
        Document = account;
        IsRecovered = recovered;

        var id = new ClaimsIdentity("TERA Arise", ClaimTypes.Email, ClaimTypes.Role);

        id.AddClaim(new(ClaimTypes.Email, account.Email.Address));

        foreach (var (access, claim) in _claims.Span)
            if (account.Access >= access)
                id.AddClaim(claim);

        AddIdentity(id);
    }
}
