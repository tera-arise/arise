using Arise.Server.Web.Authentication;
using Arise.Server.Web.Cryptography;
using Arise.Server.Web.Email;

namespace Arise.Server.Web.Controllers.Api;

internal sealed class AccountsController : ApiController
{
    // TODO: Add rate limiting throughout.

    [BindProperty]
    [FromServices]
    public required IClock Clock { get; init; }

    [BindProperty]
    [FromServices]
    public required IDocumentStore DocumentStore { get; init; }

    [BindProperty]
    [FromServices]
    public required EmailSender EmailSender { get; init; }

    [BindProperty]
    [FromServices]
    public required IOptionsSnapshot<WebOptions> Options { get; init; }

    [AllowAnonymous]
    [HttpPost]
    public async ValueTask<IActionResult> CreateAsync(AccountsCreateRequest body, CancellationToken cancellationToken)
    {
        if (User.Identity!.IsAuthenticated)
            return StatusCode(StatusCodes.Status403Forbidden);

        var normalized = body.Address.Normalize().ToUpperInvariant();

        var token = TokenGenerator.GenerateToken();
        var options = Options.Value;
        var now = Clock.GetCurrentInstant();
        var end = now + options.AccountVerificationTime;

        var strategy = PasswordStrategyProvider.GetLatestStrategy();
        var salt = strategy.GenerateSalt();

        var key = TokenGenerator.GenerateToken();

        var account = new AccountDocument
        {
            Email = new()
            {
                Address = normalized,
                Verification = new()
                {
                    Value = token,
                    Period = new(now, end),
                },
            },
            Password = new()
            {
                Kind = PasswordStrategyProvider.GetKind(strategy),
                Salt = salt,
                Hash = strategy.CalculateHash(body.Password, salt),
            },
            Access = AccountAccess.User,
            SessionTicket = new()
            {
                Value = key,
                Period = new(now, now + options.AccountSessionKeyTime),
            },
        };

        await using (var session = DocumentStore.LightweightSession())
        {
            session.Insert(account);

            try
            {
                await session.SaveChangesAsync(cancellationToken);
            }
            catch (DocumentAlreadyExistsException)
            {
                // Email address already taken.
                return UnprocessableEntity();
            }
        }

        EmailSender.EnqueueEmail(
            normalized,
            "Email Address Verification",
            $"""
            Welcome to {ThisAssembly.GameTitle}.

            To verify your email address, use this token in the launcher: {token}

            The token will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}
            """);

        return Ok(new AccountsCreateResponse
        {
            SessionTicket = key,
        });
    }

    [HttpPatch]
    public async ValueTask<IActionResult> SendAsync(AccountDocument account, CancellationToken cancellationToken)
    {
        var email = account.Email;

        if (email.Verification == null)
            return BadRequest();

        var token = TokenGenerator.GenerateToken();
        var now = Clock.GetCurrentInstant();
        var end = now + Options.Value.AccountVerificationTime;

        email.Verification = new()
        {
            Value = token,
            Period = new(now, end),
        };

        if (!await UpdateAccountAsync(account, cancellationToken))
            return Conflict();

        EmailSender.EnqueueEmail(
            email.Address,
            "Email Address Verification",
            $"""
            Welcome to {ThisAssembly.GameTitle}.

            To verify your email address, use this token in the launcher: {token}

            The token will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}
            """);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> VerifyAsync(
        AccountDocument account, AccountsVerifyRequest body, CancellationToken cancellationToken)
    {
        var now = Clock.GetCurrentInstant();

        bool MatchToken([NotNullWhen(true)] AccountToken? token)
        {
            return (token?.Period.Contains(now) ?? false) && body.Token == token.Value;
        }

        var email = account.Email;
        var change = account.ChangingEmail;
        var deletion = account.Deletion;

        if (MatchToken(change?.Verification))
        {
            account.Email = new()
            {
                Address = change.Address,
                Verification = null,
            };
            account.ChangingEmail = null;
        }
        else if (MatchToken(email.Verification))
            email.Verification = null;
        else if (MatchToken(deletion?.Verification))
            deletion.Verification = null;
        else
            return Gone();

        bool saved;

        try
        {
            saved = await UpdateAccountAsync(account, cancellationToken);
        }
        catch (DocumentAlreadyExistsException)
        {
            // This indicates that the unique constraint on AccountDocument.Email.Address was violated; that is, the
            // email address the user is changing to is already in use by another account.
            return UnprocessableEntity();
        }

        return saved ? NoContent() : Conflict();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> UpdateAsync(
        AccountDocument account, AccountsUpdateRequest body, CancellationToken cancellationToken)
    {
        var email = account.Email;

        if (body.Address is string address)
        {
            // Unverified accounts cannot change their email address.
            if (email.Verification != null)
                return BadRequest();

            var token = TokenGenerator.GenerateToken();
            var now = Clock.GetCurrentInstant();
            var end = now + Options.Value.AccountVerificationTime;

            account.ChangingEmail = new()
            {
                Address = address.Normalize().ToUpperInvariant(),
                Verification = new()
                {
                    Value = token,
                    Period = new(now, end),
                },
            };

            if (!await UpdateAccountAsync(account, cancellationToken))
                return Conflict();

            EmailSender.EnqueueEmail(
                email.Address,
                "Email Address Change Verification",
                $"""
                An email address change was recently requested for your {ThisAssembly.GameTitle} account.

                The new email address is: {address}

                To confirm the change, use this token in the launcher: {token}

                The token will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}

                If you did not initiate this request, please change your password immediately.
                """);
        }

        if (body.Password is string password)
        {
            var strategy = PasswordStrategyProvider.GetLatestStrategy();
            var salt = strategy.GenerateSalt();

            account.Password = new()
            {
                Kind = PasswordStrategyProvider.GetKind(strategy),
                Salt = salt,
                Hash = strategy.CalculateHash(password, salt),
            };

            if (!await UpdateAccountAsync(account, cancellationToken))
                return Conflict();

            EmailSender.EnqueueEmail(
                email.Address,
                "Password Change",
                $"""
                A password change was recently performed for your {ThisAssembly.GameTitle} account.

                If you did not perform this change, please change your password immediately.
                """);
        }

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPatch]
    public async ValueTask<IActionResult> RecoverAsync(AccountsRecoverRequest body, CancellationToken cancellationToken)
    {
        if (User.Identity!.IsAuthenticated)
            return StatusCode(StatusCodes.Status403Forbidden);

        var normalized = body.Address.Normalize().ToUpperInvariant();
        var account = default(AccountDocument);

        await using (var session = DocumentStore.QuerySession())
            account = await session
                .Query<AccountDocument>()
                .SingleOrDefaultAsync(account => account.Email.Address == normalized, cancellationToken);

        if (account != null)
        {
            var strategy = PasswordStrategyProvider.GetLatestStrategy();
            var salt = strategy.GenerateSalt();
            var password = PasswordStrategy.GeneratePassword();

            var now = Clock.GetCurrentInstant();
            var end = now + Options.Value.AccountRecoveryTime;

            account.Recovery = new()
            {
                Password = new()
                {
                    Kind = PasswordStrategyProvider.GetKind(strategy),
                    Salt = salt,
                    Hash = strategy.CalculateHash(password, salt),
                },
                Period = new(now, end),
            };

            if (!await UpdateAccountAsync(account, cancellationToken))
                return Conflict();

            EmailSender.EnqueueEmail(
                normalized,
                "Password Recovery",
                $"""
                Password recovery was recently requested for your {ThisAssembly.GameTitle} account.

                A temporary password has been generated for you: {password}

                The temporary password will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}

                If you log in with the above password, it will replace your current password.

                If you log in with your normal password, the temporary password will be removed from your account.

                If you did not initiate this request, you can safely ignore this message.
                """);
        }

        // https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#password-recovery
        return NoContent();
    }

    [HttpDelete]
    public async ValueTask<IActionResult> DeleteAsync(AccountDocument account, CancellationToken cancellationToken)
    {
        if (account.Deletion != null)
            return BadRequest();

        var token = TokenGenerator.GenerateToken();
        var options = Options.Value;
        var now = Clock.GetCurrentInstant();
        var end = now + options.AccountVerificationTime;

        account.Deletion = new()
        {
            Period = new(now, now + options.AccountDeletionTime),
            Verification = new()
            {
                Value = token,
                Period = new(now, end),
            },
        };

        if (!await UpdateAccountAsync(account, cancellationToken))
            return Conflict();

        EmailSender.EnqueueEmail(
            account.Email.Address,
            "Account Deletion Verification",
            $"""
            Deletion of your {ThisAssembly.GameTitle} account was recently requested.

            To confirm account deletion, use this token in the launcher: {token}

            The token will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}

            If you did not initiate this request, please change your password immediately.
            """);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> RestoreAsync(AccountDocument account, CancellationToken cancellationToken)
    {
        if (account.Deletion is null or { Verification: not null })
            return BadRequest();

        account.Deletion = null;

        return await UpdateAccountAsync(account, cancellationToken) ? NoContent() : Conflict();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> AuthenticateAsync(
        AccountClaimsPrincipal principal, IHostEnvironment environment, CancellationToken cancellationToken)
    {
        var account = principal.Document;
        var now = Clock.GetCurrentInstant();

        // Do some housekeeping of account state while we are here...

        var verifying = account.Email.Verification != null;
        var changing = false;

        if (account.ChangingEmail is AccountEmailChange change)
        {
            if (change.Verification.Period.Contains(now))
                changing = true;
            else
                account.ChangingEmail = null; // Clear expired email address change request.
        }

        if (principal.IsRecovered)
            account.Password = account.Recovery!.Password;

        account.Recovery = null; // Clear used/expired recovery password.

        var deleting = false;

        if (account.Deletion is AccountDeletion deletion)
        {
            if (deletion.Verification is AccountToken verification && !verification.Period.Contains(now))
                account.Deletion = null; // Clear expired deletion request.
            else if (deletion.Verification == null)
                deleting = true;
        }

        var reason = default(string);

        if (account.Ban is AccountBan ban)
        {
            if (ban.Period.Contains(now))
                reason = ban.Reason;
            else
                account.Ban = null; // Clear expired ban.
        }

        // Accounts that are banned or in the process of being deleted cannot access the world service. We also prevent
        // unverified accounts from accessing it since the user could have signed up with a wrong email address and
        // might otherwise not notice until they have made significant progress in the game.
        var key = (environment.IsDevelopment() || !verifying) && !deleting && reason == null
            ? TokenGenerator.GenerateToken()
            : null;

        // Save new key or clear the previous one (for ban or deletion).
        account.SessionTicket = key == null ? null : new()
        {
            Value = key,
            Period = new(now, now + Options.Value.AccountSessionKeyTime),
        };

        return await UpdateAccountAsync(account, cancellationToken)
            ? Ok(new AccountsAuthenticateResponse
            {
                IsVerifying = verifying,
                IsChangingEmail = changing,
                IsRecovered = principal.IsRecovered,
                IsDeleting = deleting,
                BanReason = reason,
                SessionTicket = key,
            })
            : Conflict();
    }

    private async ValueTask<bool> UpdateAccountAsync(AccountDocument account, CancellationToken cancellationToken)
    {
        await using var session = DocumentStore.LightweightSession();

        session.UpdateExpectedVersion(account, account.Version);

        try
        {
            await session.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException e)
        {
            Console.WriteLine(e);

            return false;
        }

        return true;
    }
}
