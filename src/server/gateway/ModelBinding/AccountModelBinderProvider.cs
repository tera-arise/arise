using Arise.Server.Gateway.Authentication;

namespace Arise.Server.Gateway.ModelBinding;

internal sealed class AccountModelBinderProvider : IModelBinderProvider
{
    public static AccountModelBinderProvider Instance { get; } = new();

    private static readonly FrozenSet<Type> _types = new[]
    {
        typeof(AccountDocument),
        typeof(AccountClaimsPrincipal),
    }.ToFrozenSet();

    public static void Register(MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, Instance);

        var detailsProviders = options.ModelMetadataDetailsProviders;

        foreach (var type in _types)
        {
            detailsProviders.Add(new BindingSourceMetadataProvider(type, BindingSource.Special));
            detailsProviders.Add(new SuppressChildValidationMetadataProvider(type));
        }
    }

    private AccountModelBinderProvider()
    {
    }

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return _types.Contains(context.Metadata.ModelType) ? AccountModelBinder.Instance : null;
    }
}
