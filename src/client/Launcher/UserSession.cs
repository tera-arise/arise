namespace Arise.Client.Launcher;

internal sealed class UserSession
{
    public event Action? StatusChanged;

    public string? AccountName { get; private set; }

    public string? SessionTicket { get; private set; }

    public bool IsVerified { get; private set; }

    public bool IsLoggedIn => AccountName is not null;

    public void Login(string accountName, AccountsAuthenticateResponse response)
    {
        AccountName = accountName;
        SessionTicket = response.SessionTicket;
        IsVerified = !response.IsVerifying;

        StatusChanged?.Invoke();
    }

    public void Logout()
    {
        AccountName = null;
        SessionTicket = null;
        IsVerified = false;

        StatusChanged?.Invoke();
    }
}
