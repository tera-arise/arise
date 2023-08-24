using Arise.Server.Web.Controllers.Api;

namespace Arise.Server.Web.Controllers;

internal sealed class InternalControllerFeatureProvider : ControllerFeatureProvider
{
    public static InternalControllerFeatureProvider Instance { get; } = new();

    private static readonly FrozenSet<Type> _types = new[]
    {
        typeof(ApiController),
        typeof(WebController),
    }.ToFrozenSet();

    protected override bool IsController(TypeInfo typeInfo)
    {
        return _types.Any(typeInfo.IsSubclassOf) || base.IsController(typeInfo);
    }
}
