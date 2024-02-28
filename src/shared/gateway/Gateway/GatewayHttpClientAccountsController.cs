namespace Arise.Gateway;

public sealed class GatewayHttpClientAccountsController : GatewayHttpClientController
{
    internal GatewayHttpClientAccountsController(GatewayHttpClient client)
        : base(client, "Accounts")
    {
    }

    public ValueTask CreateAsync(AccountsCreateRequest request)
    {
        return SendAsync(HttpMethod.Post, "Create", request, Context.AccountsCreateRequest, credentials: null);
    }

    public ValueTask SendVerificationAsync(string email, string password)
    {
        return SendAsync(HttpMethod.Patch, "SendVerification", credentials: (email, password));
    }

    public ValueTask VerifyAsync(string email, string password, AccountsVerifyRequest request)
    {
        return SendAsync(
            HttpMethod.Patch, "Verify", request, Context.AccountsVerifyRequest, credentials: (email, password));
    }

    public ValueTask<AccountsVerifyEmailChangeResponse> VerifyEmailChangeAsync(
        string email, string password, AccountsVerifyEmailChangeRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            "VerifyEmailChange",
            request,
            Context.AccountsVerifyEmailChangeRequest,
            Context.AccountsVerifyEmailChangeResponse,
            credentials: (email, password));
    }

    public ValueTask VerifyDeletionAsync(string email, string password, AccountsVerifyDeletionRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            "VerifyDeletion",
            request,
            Context.AccountsVerifyDeletionRequest,
            credentials: (email, password));
    }

    public ValueTask ChangeEmailAsync(string email, string password, AccountsChangeEmailRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            "ChangeEmail",
            request,
            Context.AccountsChangeEmailRequest,
            credentials: (email, password));
    }

    public ValueTask ChangePasswordAsync(string email, string password, AccountsChangePasswordRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            "ChangePassword",
            request,
            Context.AccountsChangePasswordRequest,
            credentials: (email, password));
    }

    public ValueTask RecoverPasswordAsync(AccountsRecoverPasswordRequest request)
    {
        return SendAsync(
            HttpMethod.Patch, "RecoverPassword", request, Context.AccountsRecoverPasswordRequest, credentials: null);
    }

    public ValueTask DeleteAsync(string email, string password)
    {
        return SendAsync(HttpMethod.Patch, "Delete", credentials: (email, password));
    }

    public ValueTask CancelDeletionAsync(string email, string password)
    {
        return SendAsync(HttpMethod.Patch, "CancelDeletion", credentials: (email, password));
    }

    public ValueTask<AccountsAuthenticateResponse> AuthenticateAsync(string email, string password)
    {
        return SendAsync(
            HttpMethod.Patch, "Authenticate", Context.AccountsAuthenticateResponse, credentials: (email, password));
    }
}
