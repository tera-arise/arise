namespace Arise.Gateway;

public sealed class GatewayHttpClient
{
    public static JsonSerializerContext JsonContext => GatewayJsonSerializerContext.Default;

    public GatewayHttpClientAccountsController Accounts { get; }

    public GatewayHttpClientLauncherController Launcher { get; }

    private readonly HttpClient _client;

    public GatewayHttpClient(HttpClient client)
    {
        Accounts = new(this);
        Launcher = new(this);
        _client = client;
    }

    internal async ValueTask<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string controller,
        string action,
        (string Email, string Password)? credentials,
        HttpContent? content)
    {
        using var request = new HttpRequestMessage(method, $"{controller}/{action}")
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
}
