namespace Arise.Gateway;

public interface IGatewayClient
{
    private const string EmailHeader = "Arise-Email";

    private const string PasswordHeader = "Arise-Password";

    public static JsonSerializerContext JsonContext => GatewayJsonSerializerContext.Default;

    [Post("/Accounts/Create")]
    public abstract Task<IApiResponse<AccountsCreateResponse>> CreateAccountAsync([Body] AccountsCreateRequest body);

    [Patch("/Accounts/Send")]
    public abstract Task<IApiResponse> SendAccountEmailAsync(
        [Header(EmailHeader)] string email, [Header(PasswordHeader)] string password);

    [Patch("/Accounts/Verify")]
    public abstract Task<IApiResponse> VerifyAccountTokenAsync(
        [Header(EmailHeader)] string email,
        [Header(PasswordHeader)] string password,
        [Body] AccountsVerifyRequest body);

    [Patch("/Accounts/Update")]
    public abstract Task<IApiResponse> UpdateAccountAsync(
        [Header(EmailHeader)] string email,
        [Header(PasswordHeader)] string password,
        [Body] AccountsUpdateRequest body);

    [Patch("/Accounts/Recover")]
    public abstract Task<IApiResponse> RecoverAccountAsync([Body] AccountsRecoverRequest body);

    [Delete("/Accounts/Delete")]
    public abstract Task<IApiResponse> RecoverAccountAsync(
        [Header(EmailHeader)] string email, [Header(PasswordHeader)] string password);

    [Patch("/Accounts/Delete")]
    public abstract Task<IApiResponse> DeleteAccountAsync(
        [Header(EmailHeader)] string email, [Header(PasswordHeader)] string password);

    [Patch("/Accounts/Restore")]
    public abstract Task<IApiResponse> RestoreAccountAsync(
        [Header(EmailHeader)] string email, [Header(PasswordHeader)] string password);

    [Patch("/Accounts/Authenticate")]
    public abstract Task<IApiResponse<AccountsAuthenticateResponse>> AuthenticateAccountAsync(
        [Header(EmailHeader)] string email, [Header(PasswordHeader)] string password);

    [Get("/News/List")]
    public abstract Task<IApiResponse<NewsListResponse>> ListNewsAsync([Body] NewsListRequest body);

    [Get("/News/Get")]
    public abstract Task<IApiResponse<NewsGetResponse>> GetNewsAsync([Body] NewsGetRequest body);

    [Get("/Version/Check")]
    public abstract Task<IApiResponse<VersionCheckResponse>> CheckVersionAsync();
}
