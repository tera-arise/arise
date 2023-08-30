namespace Arise.Server.Web.Email;

internal sealed class DelegatingSendGridClient : ISendGridClient
{
    public string UrlPath
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public string Version
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public string MediaType
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    private static readonly Task<Response> _empty = Task.FromResult(
        new Response(
            HttpStatusCode.OK,
            new ReadOnlyMemoryContent(ReadOnlyMemory<byte>.Empty),
            new HttpResponseMessage().Headers));

    private readonly SendGridClient? _client;

    public DelegatingSendGridClient(HttpClient client, IOptions<WebOptions> options)
    {
        // If no API key is available, sending will just be a no-op.
        if (options.Value.SendGridKey is { } key)
            _client = new(
                client,
                new()
                {
                    ApiKey = key,
                    HttpErrorAsException = true,
                });
    }

    [RegisterServices]
    internal static void Register(IServiceCollection services)
    {
        _ = services
            .AddTransient<ISendGridClient>(
                static provider => provider.GetRequiredService<DelegatingSendGridClient>())
            .AddHttpClient<DelegatingSendGridClient>();
    }

    public AuthenticationHeaderValue AddAuthorization(KeyValuePair<string, string> header)
    {
        throw new NotSupportedException();
    }

    public Task<Response> MakeRequest(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<Response> RequestAsync(
        BaseClient.Method method,
        string? requestBody = null,
        string? queryParams = null,
        string? urlPath = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<Response> SendEmailAsync(SendGridMessage msg, CancellationToken cancellationToken = default)
    {
        return _client != null ? _client.SendEmailAsync(msg, cancellationToken) : _empty;
    }
}
