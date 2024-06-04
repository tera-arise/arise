// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Arise.Client.Launcher;

internal sealed class UserSession
{
    /// <summary>
    /// Fired when any property changes and contextually with login and logout events.
    /// </summary>
    public event Action? StatusChanged;

    /// <summary>
    /// Fired when user is successfully logged in.
    /// </summary>
    public event Action<string>? LoggedIn;

    /// <summary>
    /// Fired when user is logged out.
    /// </summary>
    public event Action? LoggedOut;

    public string? AccountName { get; private set; }

    public string? SessionTicket { get; private set; }

    public bool IsVerified { get; private set; }

    public bool IsChangingEmail { get; private set; }

    public string? Password { get; private set; }

    public bool IsLoggedIn => AccountName is not null;

    public void Login(string accountName, AccountsAuthenticateResponse response, string? password = null)
    {
        AccountName = accountName;
        SessionTicket = response.SessionTicket;
        IsVerified = !response.IsVerifying;
        IsChangingEmail = response.IsChangingEmail;

        if ((!string.IsNullOrEmpty(password) && !IsVerified)
            || IsChangingEmail)
        {
            Password = password;
        }

        LoggedIn?.Invoke(AccountName);
        StatusChanged?.Invoke();
    }

    public void Logout()
    {
        AccountName = null;
        SessionTicket = null;
        IsVerified = false;
        Password = null;
        IsChangingEmail = false;

        LoggedOut?.Invoke();
        StatusChanged?.Invoke();
    }

    public void BeginEmailChange()
    {
        IsChangingEmail = true;

        StatusChanged?.Invoke();
    }

    public void VerifyEmailChange(string newEmail)
    {
        IsChangingEmail = false;
        Password = null;
        AccountName = newEmail;

        StatusChanged?.Invoke();
    }

    public void Verify()
    {
        IsVerified = true;
        Password = null;

        StatusChanged?.Invoke();
    }
}
