using Arise.Server.Web.Models.Api.Accounts;
using Arise.Server.Web.Models.Api.News;
using Arise.Server.Web.Models.Api.Version;

namespace Arise.Server.Web.Models.Api;

[JsonSerializable(typeof(AccountsAuthenticateResponse))]
[JsonSerializable(typeof(AccountsCreateRequest))]
[JsonSerializable(typeof(AccountsCreateResponse))]
[JsonSerializable(typeof(AccountsRecoverRequest))]
[JsonSerializable(typeof(AccountsUpdateRequest))]
[JsonSerializable(typeof(AccountsVerifyRequest))]
[JsonSerializable(typeof(NewsListResponse))]
[JsonSerializable(typeof(VersionCheckResponse))]
[JsonSourceGenerationOptions(UseStringEnumConverter = true, WriteIndented = true)]
internal sealed partial class ApiJsonSerializerContext : JsonSerializerContext
{
}
