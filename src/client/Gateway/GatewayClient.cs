namespace Arise.Client.Gateway;

internal sealed class GatewayClient
{
    public IGatewayClient Rest { get; }

    public Uri? BaseAddress
    {
        get => _client.BaseAddress;
        set => _client.BaseAddress = value;
    }

    private readonly HttpClient _client;

    public GatewayClient(HttpClient client)
    {
        Rest = RestService.For<IGatewayClient>(
            client, new RefitSettings(new SystemTextJsonContentSerializer(IGatewayClient.JsonContext.Options)));
        _client = client;
    }
}
