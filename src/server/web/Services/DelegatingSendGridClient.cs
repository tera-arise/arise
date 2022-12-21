namespace Arise.Server.Web.Services;

public sealed class DelegatingSendGridClient : ISendGridClient
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

    private readonly SendGridClient? _client;

    public DelegatingSendGridClient(HttpClient client, IOptions<WebOptions> options)
    {
        // If no API key is available, sending will just be a no-op.
        if (options.Value.SendGridKey is string key)
            _client = new(
                client,
                new()
                {
                    ApiKey = key,
                    HttpErrorAsException = true,
                });
    }

    public AuthenticationHeaderValue AddAuthorization(KeyValuePair<string, string> header)
    {
        throw new NotSupportedException();
    }

    public Task<Response> MakeRequest(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    [SuppressMessage("", "CA1054")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/6371
    public Task<Response> RequestAsync(
        BaseClient.Method method,
        string? requestBody = null,
        string? queryParams = null,
        string? urlPath = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    [SuppressMessage("", "CA2000")]
    public Task<Response> SendEmailAsync(SendGridMessage msg, CancellationToken cancellationToken = default)
    {
        return _client != null
            ? _client.SendEmailAsync(msg, cancellationToken)
            : cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<Response>(cancellationToken)
                : Task.FromResult(
                    new Response(
                        HttpStatusCode.OK,
                        new ReadOnlyMemoryContent(ReadOnlyMemory<byte>.Empty),
                        new HttpResponseMessage().Headers));
    }
}
