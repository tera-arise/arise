namespace Arise.Server.Web.Authentication;

public sealed class AccountClaimsPrincipal : ClaimsPrincipal
{
    public AccountDocument Document { get; }

    public bool IsRecovered { get; }

    private static readonly ReadOnlyMemory<AccountAccess> _levels = Enum.GetValues<AccountAccess>().Order().ToArray();

    public AccountClaimsPrincipal(AccountDocument account, bool recovered)
    {
        Document = account;
        IsRecovered = recovered;

        var id = new ClaimsIdentity("TERA Arise", ClaimTypes.Email, ClaimTypes.Role);

        id.AddClaim(new(ClaimTypes.Email, account.Email.Address));

        foreach (var access in _levels.Span)
            if (account.Access >= access)
                id.AddClaim(new(ClaimTypes.Role, access.ToString()));

        AddIdentity(id);
    }
}
