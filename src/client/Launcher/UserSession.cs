namespace Arise.Client.Launcher;

internal sealed class UserSession
{
    public event Action? StatusChanged;

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

        StatusChanged?.Invoke();
    }

    public void Logout()
    {
        AccountName = null;
        SessionTicket = null;
        IsVerified = false;
        Password = null;
        IsChangingEmail = false;

        StatusChanged?.Invoke();
    }

    public void BeginEmailChange()
    {
        IsChangingEmail = true;

        StatusChanged?.Invoke();
    }

    public void VerifyEmailChange()
    {
        IsChangingEmail = false;
        Password = null;

        StatusChanged?.Invoke();
    }

    public void Verify()
    {
        IsVerified = true;
        Password = null;

        StatusChanged?.Invoke();
    }
}
