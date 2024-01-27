namespace Arise.Gateway;

public sealed class GatewayHttpClient
{
    public static JsonSerializerContext JsonContext => _context;

    private static readonly GatewayJsonSerializerContext _context = GatewayJsonSerializerContext.Default;

    private readonly HttpClient _client;

    public GatewayHttpClient(HttpClient client)
    {
        _client = client;
    }

    private async ValueTask<HttpResponseMessage> SendCoreAsync(
        HttpMethod method, string path, (string Email, string Password)? credentials, HttpContent? content)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = content,
        };

        if (credentials is { } creds)
        {
            request.Headers.Add("Arise-Email", creds.Email);
            request.Headers.Add("Arise-Password", creds.Password);
        }

        try
        {
            return (await _client.SendAsync(request).ConfigureAwait(false)).EnsureSuccessStatusCode();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException { } tex)
        {
            ExceptionDispatchInfo.Throw(tex);

            throw new UnreachableException();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            throw new GatewayHttpException("A gateway communication exception occurred.", ex);
        }
    }

    private static async ValueTask<TResponse> DeserializeResponseAsync<TResponse>(
        HttpResponseMessage response, JsonTypeInfo<TResponse> typeInfo)
        where TResponse : class
    {
        return (await response.Content.ReadFromJsonAsync(typeInfo).ConfigureAwait(false)) ??
            throw new JsonException("Unexpected null token.");
    }

    private async ValueTask SendAsync(HttpMethod method, string path, (string Email, string Password)? credentials)
    {
        (await SendCoreAsync(method, path, credentials, content: null).ConfigureAwait(false)).Dispose();
    }

    private async ValueTask SendAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        (string Email, string Password)? credentials)
        where TRequest : class
    {
        using var content = JsonContent.Create(request, requestTypeInfo);

        (await SendCoreAsync(method, path, credentials, content).ConfigureAwait(false)).Dispose();
    }

    private async ValueTask<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        JsonTypeInfo<TResponse> responseTypeInfo,
        (string Email, string Password)? credentials)
        where TResponse : class
    {
        using var response = await SendCoreAsync(method, path, credentials, content: null).ConfigureAwait(false);

        return await DeserializeResponseAsync(response, responseTypeInfo).ConfigureAwait(false);
    }

    public ValueTask CreateAccountAsync(AccountsCreateRequest request)
    {
        return SendAsync(
            HttpMethod.Post,
            "/Accounts/Create",
            request,
            _context.AccountsCreateRequest,
            credentials: null);
    }

    public ValueTask SendAccountEmailAsync(string email, string password)
    {
        return SendAsync(HttpMethod.Patch, "/Accounts/Send", credentials: (email, password));
    }

    public ValueTask VerifyAccountTokenAsync(string email, string password, AccountsVerifyRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            "/Accounts/Verify",
            request,
            _context.AccountsVerifyRequest,
            credentials: (email, password));
    }

    public ValueTask UpdateAccountAsync(string email, string password, AccountsUpdateRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            "/Accounts/Update",
            request,
            _context.AccountsUpdateRequest,
            credentials: (email, password));
    }

    public ValueTask RecoverAccountAsync(AccountsRecoverRequest request)
    {
        return SendAsync(
            HttpMethod.Patch, "/Accounts/Recover", request, _context.AccountsRecoverRequest, credentials: null);
    }

    public ValueTask DeleteAccountAsync(string email, string password)
    {
        return SendAsync(HttpMethod.Patch, "/Accounts/Delete", credentials: (email, password));
    }

    public ValueTask RestoreAccountAsync(string email, string password)
    {
        return SendAsync(HttpMethod.Patch, "/Accounts/Restore", credentials: (email, password));
    }

    public ValueTask<AccountsAuthenticateResponse> AuthenticateAccountAsync(string email, string password)
    {
        return SendAsync(
            HttpMethod.Patch,
            "/Accounts/Authenticate",
            _context.AccountsAuthenticateResponse,
            credentials: (email, password));
    }

    public ValueTask<LauncherHelloResponse> HelloLauncherAsync()
    {
        return SendAsync(HttpMethod.Get, "/Launcher/Hello", _context.LauncherHelloResponse, credentials: null);
    }
}
