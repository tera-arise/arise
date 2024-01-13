namespace Arise.Server.Gateway.Controllers;

internal sealed class InternalControllerFeatureProvider : ControllerFeatureProvider
{
    public static InternalControllerFeatureProvider Instance { get; } = new();

    protected override bool IsController(TypeInfo typeInfo)
    {
        return typeInfo.IsSubclassOf(typeof(ApiController)) || base.IsController(typeInfo);
    }
}
