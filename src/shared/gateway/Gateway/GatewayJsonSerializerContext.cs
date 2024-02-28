namespace Arise.Gateway;

[JsonSerializable(typeof(AccountsAuthenticateResponse))]
[JsonSerializable(typeof(AccountsChangeEmailRequest))]
[JsonSerializable(typeof(AccountsChangePasswordRequest))]
[JsonSerializable(typeof(AccountsCreateRequest))]
[JsonSerializable(typeof(AccountsRecoverPasswordRequest))]
[JsonSerializable(typeof(AccountsVerifyRequest))]
[JsonSerializable(typeof(AccountsVerifyDeletionRequest))]
[JsonSerializable(typeof(AccountsVerifyEmailChangeRequest))]
[JsonSerializable(typeof(AccountsVerifyEmailChangeResponse))]
[JsonSerializable(typeof(LauncherHelloResponse))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true)]
internal sealed partial class GatewayJsonSerializerContext : JsonSerializerContext
{
}
