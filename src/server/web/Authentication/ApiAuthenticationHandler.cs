using Arise.Server.Web.Cryptography;

namespace Arise.Server.Web.Authentication;

internal sealed class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
{
    public const string Name = "Api";

    private readonly IDocumentStore _store;

    private readonly IClock _clock;

    public ApiAuthenticationHandler(
        IOptionsMonitor<ApiAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IDocumentStore store,
        IClock clock)
        : base(options, logger, encoder)
    {
        _store = store;
        _clock = clock;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headers = Request.Headers;

        if (!headers.TryGetValue("TERA-Arise-Address", out var addressHeader) ||
            !headers.TryGetValue("TERA-Arise-Password", out var passwordHeader))
            return AuthenticateResult.NoResult();

        static AuthenticateResult Fail(string message)
        {
            return AuthenticateResult.Fail(message);
        }

        if (addressHeader.Count != 1 || passwordHeader.Count != 1)
            return Fail("Authentication header format is invalid.");

        var providedAddress = addressHeader[0]!;

        if (!EmailValidator.Validate(providedAddress, allowTopLevelDomains: true, allowInternational: true))
            return Fail("Email address format is invalid.");

        var providedPassword = passwordHeader[0]!;

        if (!PasswordStrategy.IsPasswordValid(providedPassword))
            return Fail("Password format is invalid.");

        var normalizedAddress = providedAddress.Normalize().ToUpperInvariant();
        var account = default(AccountDocument);

        await using (var session = _store.QuerySession())
            account = await session
                .Query<AccountDocument>()
                .SingleOrDefaultAsync(account => account.Email.Address == normalizedAddress);

        // https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#login
        if (account == null)
            return Fail("Email address or password is incorrect.");

        bool MatchPassword(AccountPassword password)
        {
            return CryptographicOperations.FixedTimeEquals(
                PasswordStrategy.GetStrategy(password.Kind).CalculateHash(providedPassword, password.Salt),
                password.Hash);
        }

        var principal = default(AccountClaimsPrincipal);

        if (MatchPassword(account.Password))
            principal = new(account, recovered: false);
        else if (account.Recovery is AccountRecovery recovery &&
            recovery.Period.Contains(_clock.GetCurrentInstant()) &&
            MatchPassword(recovery.Password))
            principal = new(account, recovered: true);

        return principal != null
            ? AuthenticateResult.Success(new(principal, Name))
            : Fail("Email address or password is incorrect.");
    }
}
