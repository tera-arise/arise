namespace Arise.Client.Gateway;

internal sealed class GatewayClient
{
    public GatewayHttpClient Rest { get; }

    public Uri? BaseAddress
    {
        get => _client.BaseAddress;
        set => _client.BaseAddress = value;
    }

    private readonly HttpClient _client;

    public GatewayClient(HttpClient client)
    {
        Rest = new(client);
        _client = client;
    }
}
