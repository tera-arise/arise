using Arise.Server.Web.Authentication;
using Arise.Server.Web.Mail;
using Arise.Server.Web.ModelBinding;
using Arise.Server.Web.News;
using Arise.Server.Web.Services;

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
            .AddTransient<ISendGridClient>(static provider => provider.GetRequiredService<DelegatingSendGridClient>())
            .AddTransient<MailSender>()
            .AddSingleton<GameDownloadProvider>()
            .AddSingleton<NewsArticleProvider>()
            .AddHostedService(static provider => provider.GetRequiredService<NewsArticleProvider>())
            .AddControllersWithViews(static opts =>
            {
                opts.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());

                AccountModelBinderProvider.Register(opts);
            })
            .AddJsonOptions(static opts =>
            {
                var json = opts.JsonSerializerOptions;

                json.AllowTrailingCommas = true;
                json.NumberHandling |=
                    JsonNumberHandling.WriteAsString |
                    JsonNumberHandling.AllowNamedFloatingPointLiterals;
                json.ReadCommentHandling = JsonCommentHandling.Skip;
                json.WriteIndented = true;

                json.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

                _ = json.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            })
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
            });
    }
}
