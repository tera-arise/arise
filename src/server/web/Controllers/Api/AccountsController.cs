using Arise.Server.Web.Authentication;
using Arise.Server.Web.Cryptography;
using Arise.Server.Web.Mail;
using Arise.Server.Web.Models.Api.Accounts;

namespace Arise.Server.Web.Controllers.Api;

public sealed class AccountsController : ApiController
{
    // TODO: Add rate limiting throughout.

    [BindProperty]
    [FromServices]
    public required IClock Clock { get; init; }

    [BindProperty]
    [FromServices]
    public required IDocumentStore Store { get; init; }

    [BindProperty]
    [FromServices]
    public required MailSender Sender { get; init; }

    [BindProperty]
    [FromServices]
    public required IOptionsSnapshot<WebOptions> Options { get; init; }

    [AllowAnonymous]
    [HttpPost]
    public async ValueTask<IActionResult> CreateAsync(AccountsCreateRequest body)
    {
        if (User.Identity!.IsAuthenticated)
            return StatusCode(StatusCodes.Status403Forbidden);

        var normalized = body.Address.Normalize().ToUpperInvariant();

        var token = TokenGenerator.GenerateToken();
        var options = Options.Value;
        var now = Clock.GetCurrentInstant();
        var end = now + options.AccountVerificationTime;

        var strategy = PasswordStrategy.GetLatestStrategy();
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
                Kind = strategy.Kind,
                Salt = salt,
                Hash = strategy.CalculateHash(body.Password, salt),
            },
            Access = AccountAccess.User,
            GameKey = new()
            {
                Value = key,
                Period = new(now, now + options.AccountKeyTime),
            },
        };

        await using (var session = Store.LightweightSession())
        {
            session.Insert(account);

            try
            {
                await session.SaveChangesAsync(CancellationToken);
            }
            catch (DocumentAlreadyExistsException)
            {
                // Email address already taken.
                return UnprocessableEntity();
            }
        }

        await Sender.SendAsync(
            normalized,
            "Email Address Verification",
            $"""
            Welcome to TERA Arise.

            To verify your email address, use this token in the launcher: {token}

            The token will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}
            """,
            CancellationToken);

        return Ok(new AccountsCreateResponse
        {
            GameKey = key,
        });
    }

    [HttpPatch]
    public async ValueTask<IActionResult> SendAsync(AccountDocument account)
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

        if (!await UpdateAccountAsync(account))
            return Conflict();

        await Sender.SendAsync(
            email.Address,
            "Email Address Verification",
            $"""
            Welcome to TERA Arise.

            To verify your email address, use this token in the launcher: {token}

            The token will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}
            """,
            CancellationToken);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> VerifyAsync(AccountDocument account, AccountsVerifyRequest body)
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
            saved = await UpdateAccountAsync(account);
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
    public async ValueTask<IActionResult> UpdateAsync(AccountDocument account, AccountsUpdateRequest body)
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

            if (!await UpdateAccountAsync(account))
                return Conflict();

            await Sender.SendAsync(
                email.Address,
                "Email Address Change Verification",
                $"""
                An email address change was recently requested for your TERA Arise account.

                The new email address is: {address}

                To confirm the change, use this token in the launcher: {token}

                The token will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}

                If you did not initiate this request, please change your password immediately.
                """,
                CancellationToken);
        }

        if (body.Password is string password)
        {
            var strategy = PasswordStrategy.GetLatestStrategy();
            var salt = strategy.GenerateSalt();

            account.Password = new()
            {
                Kind = strategy.Kind,
                Salt = salt,
                Hash = strategy.CalculateHash(password, salt),
            };

            if (!await UpdateAccountAsync(account))
                return Conflict();

            await Sender.SendAsync(
                email.Address,
                "Password Change",
                $"""
                A password change was recently performed for your TERA Arise account.

                If you did not perform this change, please change your password immediately.
                """,
                CancellationToken);
        }

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPatch]
    public async ValueTask<IActionResult> RecoverAsync(AccountsRecoverRequest body)
    {
        if (User.Identity!.IsAuthenticated)
            return StatusCode(StatusCodes.Status403Forbidden);

        var normalized = body.Address.Normalize().ToUpperInvariant();
        var account = default(AccountDocument);

        await using (var session = Store.QuerySession())
            account = await session
                .Query<AccountDocument>()
                .SingleOrDefaultAsync(account => account.Email.Address == normalized, CancellationToken);

        if (account != null)
        {
            var strategy = PasswordStrategy.GetLatestStrategy();
            var salt = strategy.GenerateSalt();
            var password = PasswordStrategy.GeneratePassword();

            var now = Clock.GetCurrentInstant();
            var end = now + Options.Value.AccountRecoveryTime;

            account.Recovery = new()
            {
                Password = new()
                {
                    Kind = strategy.Kind,
                    Salt = salt,
                    Hash = strategy.CalculateHash(password, salt),
                },
                Period = new(now, end),
            };

            if (!await UpdateAccountAsync(account))
                return Conflict();

            // TODO: This should ideally be done in a separate thread to prevent timing-based user enumeration.
            //
            // https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html#forgot-password-request
            await Sender.SendAsync(
                normalized,
                "Password Recovery",
                $"""
                Password recovery was recently requested for your TERA Arise account.

                A temporary password has been generated for you: {password}

                The temporary password will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}

                If you log in with the above password, it will replace your current password.

                If you log in with your normal password, the temporary password will be removed from your account.

                If you did not initiate this request, you can safely ignore this message.
                """,
                CancellationToken);
        }

        // https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#password-recovery
        return NoContent();
    }

    [HttpDelete]
    public async ValueTask<IActionResult> DeleteAsync(AccountDocument account)
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

        if (!await UpdateAccountAsync(account))
            return Conflict();

        await Sender.SendAsync(
            account.Email.Address,
            "Account Deletion Verification",
            $"""
            Deletion of your TERA Arise account was recently requested.

            To confirm account deletion, use this token in the launcher: {token}

            The token will expire on: {end.InUtc().Date.ToString(null, CultureInfo.InvariantCulture)}

            If you did not initiate this request, please change your password immediately.
            """,
            CancellationToken);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> RestoreAsync(AccountDocument account)
    {
        if (account.Deletion is null or { Verification: not null })
            return BadRequest();

        account.Deletion = null;

        return await UpdateAccountAsync(account) ? NoContent() : Conflict();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> AuthenticateAsync(AccountClaimsPrincipal principal)
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
        var key = !(verifying || deleting) && reason == null ? TokenGenerator.GenerateToken() : null;

        // Save new key or clear the previous one (for ban or deletion).
        account.GameKey = key == null ? null : new()
        {
            Value = key,
            Period = new(now, now + Options.Value.AccountKeyTime),
        };

        return await UpdateAccountAsync(account)
            ? Ok(new AccountsAuthenticateResponse
            {
                IsVerifying = verifying,
                IsChangingEmail = changing,
                IsRecovered = principal.IsRecovered,
                IsDeleting = deleting,
                BanReason = reason,
                GameKey = key,
            })
            : Conflict();
    }

    private async ValueTask<bool> UpdateAccountAsync(AccountDocument account)
    {
        await using var session = Store.LightweightSession();

        // TODO: https://github.com/JasperFx/marten/issues/2439
        session.Store(account, account.Version);

        try
        {
            await session.SaveChangesAsync(CancellationToken);
        }
        catch (ConcurrencyException e)
        {
            Console.WriteLine(e);

            return false;
        }

        return true;
    }
}
