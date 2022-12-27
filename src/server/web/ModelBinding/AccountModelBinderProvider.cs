using Arise.Server.Web.Authentication;

namespace Arise.Server.Web.ModelBinding;

public sealed class AccountModelBinderProvider : IModelBinderProvider
{
    public static AccountModelBinderProvider Instance { get; } = new();

    private static readonly IReadOnlySet<Type> _types = new[]
    {
        typeof(AccountDocument),
        typeof(AccountClaimsPrincipal),
    }.ToHashSet();

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
