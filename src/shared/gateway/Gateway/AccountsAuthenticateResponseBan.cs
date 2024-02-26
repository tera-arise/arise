namespace Arise.Gateway;

public sealed class AccountsAuthenticateResponseBan
{
    [JsonConverter(typeof(NodaTimeDefaultJsonConverterFactory))]
    public required Instant Expiry { get; init; }

    public required string? Reason { get; init; }
}
