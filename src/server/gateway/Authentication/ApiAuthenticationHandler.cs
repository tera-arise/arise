// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Server.Gateway.Cryptography;

namespace Arise.Server.Gateway.Authentication;

internal sealed class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
{
    public const string Name = "Api";

    private readonly IDocumentStore _store;

    public ApiAuthenticationHandler(
        IOptionsMonitor<ApiAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IDocumentStore store)
        : base(options, logger, encoder)
    {
        _store = store;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headers = Request.Headers;

        if (!headers.TryGetValue("Arise-Email", out var emailHeader) ||
            !headers.TryGetValue("Arise-Password", out var passwordHeader))
            return AuthenticateResult.NoResult();

        static AuthenticateResult Fail(string message)
        {
            return AuthenticateResult.Fail(message);
        }

        if (emailHeader.Count != 1 || passwordHeader.Count != 1)
            return Fail("Authentication header format is invalid.");

        var providedEmail = emailHeader[0]!;

        if (!EmailValidator.Validate(providedEmail, allowTopLevelDomains: true, allowInternational: true))
            return Fail("Email address format is invalid.");

        var providedPassword = passwordHeader[0]!;

        if (!PasswordStrategy.IsPasswordValid(providedPassword))
            return Fail("Password format is invalid.");

        var normalizedEmail = providedEmail.Normalize().ToUpperInvariant();
        var account = default(AccountDocument);

        await using (var session = _store.QuerySession())
            account = await session
                .Query<AccountDocument>()
                .SingleOrDefaultAsync(account => account.Email.NormalizedAddress == normalizedEmail);

        // https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#login
        if (account == null)
            return Fail("Email address or password is incorrect.");

        bool MatchPassword(AccountPassword password)
        {
            return CryptographicOperations.FixedTimeEquals(
                PasswordStrategyProvider.GetStrategy(password.Kind).CalculateHash(providedPassword, password.Salt),
                password.Hash);
        }

        var principal = default(AccountClaimsPrincipal);

        if (MatchPassword(account.Password))
            principal = new(account, recovered: false);
        else if (account.Recovery is AccountRecovery recovery &&
            TimeProvider.GetUtcNow() < recovery.Expiry &&
            MatchPassword(recovery.Password))
            principal = new(account, recovered: true);

        return principal != null
            ? AuthenticateResult.Success(new(principal, Name))
            : Fail("Email address or password is incorrect.");
    }
}
