namespace Arise.Gateway;

[JsonSerializable(typeof(AccountsAuthenticateResponse))]
[JsonSerializable(typeof(AccountsCreateRequest))]
[JsonSerializable(typeof(AccountsCreateResponse))]
[JsonSerializable(typeof(AccountsRecoverRequest))]
[JsonSerializable(typeof(AccountsUpdateRequest))]
[JsonSerializable(typeof(AccountsVerifyRequest))]
[JsonSerializable(typeof(LauncherHelloResponse))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal sealed partial class GatewayJsonSerializerContext : JsonSerializerContext
{
}
