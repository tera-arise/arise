using Arise.Server.Gateway.Authentication;
using Arise.Server.Gateway.Cryptography;
using Arise.Server.Gateway.Email;

namespace Arise.Server.Gateway.Controllers;

internal sealed class AccountsController : ApiController
{
    [BindProperty]
    [FromServices]
    public required IHostEnvironment HostEnvironment { get; init; }

    [BindProperty]
    [FromServices]
    public required TimeProvider TimeProvider { get; init; }

    [BindProperty]
    [FromServices]
    public required IDocumentStore DocumentStore { get; init; }

    [BindProperty]
    [FromServices]
    public required EmailSender EmailSender { get; init; }

    [BindProperty]
    [FromServices]
    public required IOptions<GatewayOptions> Options { get; init; }

    [AllowAnonymous]
    [HttpPost]
    public async ValueTask<IActionResult> CreateAsync(AccountsCreateRequest body, CancellationToken cancellationToken)
    {
        if (User.Identity!.IsAuthenticated)
            return Forbid();

        var normalized = body.Email.Normalize().ToUpperInvariant();

        var token = TokenGenerator.GenerateToken();
        var expiry = TimeProvider.GetUtcNow().DateTime + Options.Value.AccountVerificationTime;

        var strategy = PasswordStrategyProvider.GetLatestStrategy();
        var salt = strategy.GenerateSalt();

        var account = new AccountDocument
        {
            Email = new()
            {
                OriginalAddress = body.Email,
                NormalizedAddress = normalized,
                Verification = new()
                {
                    Value = token,
                    Expiry = expiry,
                },
            },
            Password = new()
            {
                Kind = PasswordStrategyProvider.GetKind(strategy),
                Salt = salt,
                Hash = strategy.CalculateHash(body.Password, salt),
            },
            Access = HostEnvironment.IsDevelopment() ? AccountAccess.Operator : AccountAccess.User,
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

            The token will expire on: {expiry:O}
            """);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> SendVerificationAsync(
        AccountDocument account, CancellationToken cancellationToken)
    {
        var email = account.Email;

        // Is the account already verified?
        if (email.Verification == null)
            return BadRequest();

        var token = TokenGenerator.GenerateToken();
        var expiry = TimeProvider.GetUtcNow().DateTime + Options.Value.AccountVerificationTime;

        email.Verification = new()
        {
            Value = token,
            Expiry = expiry,
        };

        if (!await UpdateAccountAsync(account, cancellationToken))
            return Conflict();

        EmailSender.EnqueueEmail(
            email.OriginalAddress,
            "Email Address Verification",
            $"""
            Welcome to {ThisAssembly.GameTitle}.

            To verify your email address, use this token in the launcher: {token}

            The token will expire on: {expiry:O}
            """);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> VerifyAsync(
        AccountDocument account, AccountsVerifyRequest body, CancellationToken cancellationToken)
    {
        var email = account.Email;

        if (email.Verification is not { } verification ||
            TimeProvider.GetUtcNow() >= verification.Expiry ||
            body.Token != verification.Value)
            return BadRequest();

        email.Verification = null;

        return await UpdateAccountAsync(account, cancellationToken) ? NoContent() : Conflict();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> VerifyEmailChangeAsync(
        AccountDocument account, AccountsVerifyEmailChangeRequest body, CancellationToken cancellationToken)
    {
        if (account.ChangingEmail is not { Verification: { } verification } change ||
            TimeProvider.GetUtcNow() >= verification.Expiry ||
            body.Token != verification.Value)
            return BadRequest();

        var original = change.OriginalAddress;

        account.Email = new()
        {
            OriginalAddress = original,
            NormalizedAddress = original.Normalize().ToUpperInvariant(),
            Verification = null,
        };
        account.ChangingEmail = null;

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

        return saved
            ? Ok(new AccountsVerifyEmailChangeResponse
            {
                Email = original,
            })
            : Conflict();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> VerifyDeletionAsync(
        AccountDocument account, AccountsVerifyDeletionRequest body, CancellationToken cancellationToken)
    {
        if (account.Deletion is not { Verification: { } verification } deletion ||
            TimeProvider.GetUtcNow() >= verification.Expiry ||
            body.Token != verification.Value)
            return BadRequest();

        deletion.Verification = null;

        if (!await UpdateAccountAsync(account, cancellationToken))
            return Conflict();

        EmailSender.EnqueueEmail(
            account.Email.OriginalAddress,
            "Account Deletion",
            $"""
            Deletion of your {ThisAssembly.GameTitle} account is now in progress.

            Your account will be deleted on: {deletion.Due:O}

            If you change your mind, please cancel the deletion in the launcher.
            """);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> ChangeEmailAsync(
        AccountDocument account, AccountsChangeEmailRequest body, CancellationToken cancellationToken)
    {
        var email = account.Email;

        // Unverified accounts cannot change their email address.
        if (!HostEnvironment.IsDevelopment() && email.Verification != null)
            return BadRequest();

        var token = TokenGenerator.GenerateToken();
        var expiry = TimeProvider.GetUtcNow().DateTime + Options.Value.AccountVerificationTime;

        account.ChangingEmail = new()
        {
            OriginalAddress = body.Email,
            Verification = new()
            {
                Value = token,
                Expiry = expiry,
            },
        };

        if (!await UpdateAccountAsync(account, cancellationToken))
            return Conflict();

        EmailSender.EnqueueEmail(
            email.OriginalAddress,
            "Email Address Change Verification",
            $"""
            An email address change was recently requested for your {ThisAssembly.GameTitle} account.

            The new email address is: {body.Email}

            To confirm the change, use this token in the launcher: {token}

            The token will expire on: {expiry:O}

            If you did not initiate this request, please change your password in the launcher immediately.
            """);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> ChangePasswordAsync(
        AccountDocument account, AccountsChangePasswordRequest body, CancellationToken cancellationToken)
    {
        var strategy = PasswordStrategyProvider.GetLatestStrategy();
        var salt = strategy.GenerateSalt();

        account.Password = new()
        {
            Kind = PasswordStrategyProvider.GetKind(strategy),
            Salt = salt,
            Hash = strategy.CalculateHash(body.Password, salt),
        };

        if (!await UpdateAccountAsync(account, cancellationToken))
            return Conflict();

        EmailSender.EnqueueEmail(
            account.Email.OriginalAddress,
            "Password Change",
            $"""
            A password change was recently performed for your {ThisAssembly.GameTitle} account.

            If you did not perform this change, please perform password recovery in the launcher immediately.
            """);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPatch]
    public async ValueTask<IActionResult> RecoverPasswordAsync(
        AccountsRecoverPasswordRequest body, CancellationToken cancellationToken)
    {
        if (User.Identity!.IsAuthenticated)
            return Forbid();

        var normalized = body.Email.Normalize().ToUpperInvariant();
        var account = default(AccountDocument);

        await using (var session = DocumentStore.QuerySession())
            account = await session
                .Query<AccountDocument>()
                .SingleOrDefaultAsync(account => account.Email.NormalizedAddress == normalized, cancellationToken);

        if (account != null)
        {
            var strategy = PasswordStrategyProvider.GetLatestStrategy();
            var salt = strategy.GenerateSalt();
            var password = PasswordStrategy.GeneratePassword();
            var expiry = TimeProvider.GetUtcNow().DateTime + Options.Value.AccountRecoveryTime;

            account.Recovery = new()
            {
                Password = new()
                {
                    Kind = PasswordStrategyProvider.GetKind(strategy),
                    Salt = salt,
                    Hash = strategy.CalculateHash(password, salt),
                },
                Expiry = expiry,
            };

            if (!await UpdateAccountAsync(account, cancellationToken))
                return Conflict();

            EmailSender.EnqueueEmail(
                normalized,
                "Password Recovery",
                $"""
                Password recovery was recently requested for your {ThisAssembly.GameTitle} account.

                A temporary password has been generated for you: {password}

                The temporary password will expire on: {expiry:O}

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
        // Has deletion already been confirmed?
        if (account.Deletion is { Verification: null })
            return BadRequest();

        var options = Options.Value;

        var token = TokenGenerator.GenerateToken();
        var now = TimeProvider.GetUtcNow().DateTime;
        var expiry = now + options.AccountVerificationTime;

        account.Deletion = new()
        {
            Due = now + options.AccountDeletionTime,
            Verification = new()
            {
                Value = token,
                Expiry = expiry,
            },
        };

        if (!await UpdateAccountAsync(account, cancellationToken))
            return Conflict();

        EmailSender.EnqueueEmail(
            account.Email.OriginalAddress,
            "Account Deletion Verification",
            $"""
            Deletion of your {ThisAssembly.GameTitle} account was recently requested.

            To confirm account deletion, use this token in the launcher: {token}

            The token will expire on: {expiry:O}

            If you did not initiate this request, please change your password in the launcher immediately.
            """);

        return NoContent();
    }

    [HttpPatch]
    public async ValueTask<IActionResult> CancelDeletionAsync(
        AccountDocument account, CancellationToken cancellationToken)
    {
        if (account.Deletion is null or { Verification: not null })
            return BadRequest();

        account.Deletion = null;

        return await UpdateAccountAsync(account, cancellationToken) ? NoContent() : Conflict();
    }

    [DisableRateLimiting]
    [HttpPatch]
    [SuppressMessage("", "CA1508")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/7164
    public async ValueTask<IActionResult> AuthenticateAsync(
        AccountClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var account = principal.Document;
        var now = TimeProvider.GetUtcNow().DateTime;

        // Do some housekeeping of account state while we are here...

        var verifying = account.Email.Verification != null;
        var changing = false;

        if (account.ChangingEmail is { } change)
        {
            if (now < change.Verification.Expiry)
                changing = true;
            else
                account.ChangingEmail = null; // Clear expired email address change request.
        }

        // If the recovery password was used, make it the actual password.
        if (principal.IsRecovered)
            account.Password = account.Recovery!.Password;

        account.Recovery = null; // Clear used/expired recovery password.

        var deletionDue = default(DateTime?);

        if (account.Deletion is { } deletion)
        {
            if (deletion.Verification is { } verification && now >= verification.Expiry)
                account.Deletion = null; // Clear expired deletion request.
            else if (deletion.Verification == null)
                deletionDue = deletion.Due;
        }

        var activeBan = default(AccountBan);

        if (account.Ban is { } ban)
        {
            if (now < ban.Expiry)
                activeBan = ban;
            else
                account.Ban = null; // Clear expired ban.
        }

        // Accounts that are banned or in the process of being deleted cannot access the world server. We also prevent
        // unverified accounts from accessing it since the user could have signed up with a wrong email address and
        // might otherwise not notice until they have made significant progress in the game.
        var ticket = HostEnvironment.IsDevelopment() || (!verifying && (deletionDue, activeBan) == (null, null))
            ? TokenGenerator.GenerateToken()
            : null;

        // Save new key or clear the previous one (for ban or deletion).
        account.SessionTicket = ticket == null ? null : new()
        {
            Value = ticket,
            Expiry = now + Options.Value.AccountAuthenticationTime,
        };

        return await UpdateAccountAsync(account, cancellationToken)
            ? Ok(new AccountsAuthenticateResponse
            {
                IsVerifying = verifying,
                IsChangingEmail = changing,
                IsRecovered = principal.IsRecovered,
                DeletionDue = deletionDue,
                Ban = activeBan != null
                    ? new()
                    {
                        Expiry = activeBan.Expiry,
                        Reason = activeBan.Reason,
                    }
                    : null,
                SessionTicket = ticket,
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
        catch (ConcurrencyException)
        {
            return false;
        }

        return true;
    }
}
