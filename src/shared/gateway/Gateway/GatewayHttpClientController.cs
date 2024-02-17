namespace Arise.Gateway;

public abstract class GatewayHttpClientController
{
    private protected static GatewayJsonSerializerContext Context => GatewayJsonSerializerContext.Default;

    private readonly GatewayHttpClient _client;

    private readonly string _controller;

    private protected GatewayHttpClientController(GatewayHttpClient client, string controller)
    {
        _client = client;
        _controller = controller;
    }

    private protected async ValueTask SendAsync(
        HttpMethod method, string action, (string Email, string Password)? credentials)
    {
        (await _client.SendAsync(method, _controller, action, credentials, content: null).ConfigureAwait(false))
            .Dispose();
    }

    private protected async ValueTask SendAsync<TRequest>(
        HttpMethod method,
        string action,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        (string Email, string Password)? credentials)
        where TRequest : class
    {
        using var content = JsonContent.Create(request, requestTypeInfo);

        (await _client.SendAsync(method, _controller, action, credentials, content).ConfigureAwait(false)).Dispose();
    }

    private protected async ValueTask<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string action,
        JsonTypeInfo<TResponse> responseTypeInfo,
        (string Email, string Password)? credentials)
        where TResponse : class
    {
        using var response = await _client.SendAsync(method, _controller, action, credentials, content: null)
            .ConfigureAwait(false);

        return await DeserializeResponseAsync(response, responseTypeInfo).ConfigureAwait(false);
    }

    private static async ValueTask<TResponse> DeserializeResponseAsync<TResponse>(
        HttpResponseMessage response, JsonTypeInfo<TResponse> typeInfo)
        where TResponse : class
    {
        return (await response.Content.ReadFromJsonAsync(typeInfo).ConfigureAwait(false)) ??
            throw new JsonException("Unexpected null token.");
    }
}
