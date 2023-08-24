using Arise.Server.Web.Authentication;

namespace Arise.Server.Web.ModelBinding;

internal sealed class AccountModelBinder : IModelBinder
{
    public static AccountModelBinder Instance { get; } = new();

    private AccountModelBinder()
    {
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // A cast failure here is a programming error as a controller action should not have an AccountDocument or
        // AccountClaimsPrincipal parameter if it allows anonymous access.
        var account = (AccountClaimsPrincipal)bindingContext.HttpContext.User;
        var result = bindingContext.ModelType == typeof(AccountDocument) ? (object)account.Document : account;

        bindingContext.ValidationState.Add(
            result,
            new()
            {
                SuppressValidation = true,
            });

        bindingContext.Result = ModelBindingResult.Success(result);

        return Task.CompletedTask;
    }
}
