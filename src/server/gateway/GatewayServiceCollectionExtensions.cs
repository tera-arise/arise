using Arise.Server.Gateway.Authentication;
using Arise.Server.Gateway.Controllers;
using Arise.Server.Gateway.Email;
using Arise.Server.Gateway.ModelBinding;
using Arise.Server.Gateway.News;
using Arise.Server.Gateway.RateLimiting;

namespace Arise.Server.Gateway;

public static class GatewayServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        return services
            .AddControllers(static opts =>
            {
                opts.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());

                AccountModelBinderProvider.Register(opts);
            })
            .AddJsonOptions(static opts =>
            {
                var json = opts.JsonSerializerOptions;

                json.WriteIndented = true;

                _ = json.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

                json.TypeInfoResolverChain.Add(IGatewayClient.JsonContext);
            })
            .ConfigureApplicationPartManager(
                static manager => manager.FeatureProviders.Add(InternalControllerFeatureProvider.Instance))
            .AddApplicationPart(typeof(ThisAssembly).Assembly)
            .Services
            .AddAuthentication()
            .AddScheme<ApiAuthenticationOptions, ApiAuthenticationHandler>(
                ApiAuthenticationHandler.Name, configureOptions: null)
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
            .AddRateLimiter(static opts =>
            {
                opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                _ = opts.AddPolicy<IPAddress, ApiRateLimiterPolicy>("Api");
            })
            .AddRouting(static opts =>
            {
                opts.LowercaseUrls = true;
                opts.LowercaseQueryStrings = true;
            })
            .AddAriseServerGateway()
            .AddHostedService(static provider => provider.GetRequiredService<NewsArticleProvider>())
            .AddHostedService(static provider => provider.GetRequiredService<EmailSender>());
    }
}