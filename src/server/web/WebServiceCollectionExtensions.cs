using Arise.Server.Web.Authentication;
using Arise.Server.Web.Controllers;
using Arise.Server.Web.Email;
using Arise.Server.Web.ModelBinding;
using Arise.Server.Web.News;

namespace Arise.Server.Web;

public static class WebServiceCollectionExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        return services
            .AddOptions<WebOptions>()
            .BindConfiguration("Web")
            .Services
            .AddHttpClient<DelegatingSendGridClient>()
            .Services
            .AddControllersWithViews(static opts =>
            {
                opts.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());

                AccountModelBinderProvider.Register(opts);
            })
            .AddJsonOptions(static opts =>
            {
                var json = opts.JsonSerializerOptions;

                json.TypeInfoResolver = GatewayClient.JsonContext;
                json.WriteIndented = true;

                _ = json.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            })
            .ConfigureApplicationPartManager(
                static manager => manager.FeatureProviders.Add(InternalControllerFeatureProvider.Instance))
            .AddApplicationPart(typeof(ThisAssembly).Assembly)
            .Services
            .AddAuthentication()
            .AddScheme<ApiAuthenticationOptions, ApiAuthenticationHandler>(ApiAuthenticationHandler.Name, null)
            .Services
            .AddAuthorization(static opts =>
                opts.AddPolicy(
                    "Api",
                    static opts =>
                        opts
                            .AddAuthenticationSchemes(ApiAuthenticationHandler.Name)
                            .RequireAuthenticatedUser()))
            .AddProblemDetails()
            .AddResponseCompression(static opts => opts.EnableForHttps = true)
            .Configure<BrotliCompressionProviderOptions>(static opts => opts.Level = CompressionLevel.SmallestSize)
            .Configure<GzipCompressionProviderOptions>(static opts => opts.Level = CompressionLevel.SmallestSize)
            .AddRouting(static opts =>
            {
                opts.LowercaseUrls = true;
                opts.LowercaseQueryStrings = true;
            })
            .AddAriseServerWeb()
            .AddHostedService(static provider => provider.GetRequiredService<NewsArticleProvider>())
            .AddHostedService(static provider => provider.GetRequiredService<EmailSender>());
    }
}
