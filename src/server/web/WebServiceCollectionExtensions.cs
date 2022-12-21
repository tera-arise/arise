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
            .AddTransient<ISendGridClient>(provider => provider.GetRequiredService<DelegatingSendGridClient>())
            .AddSingleton<GameDownloadProvider>()
            .AddControllersWithViews(
                opts => opts.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()))
            .AddJsonOptions(opts =>
            {
                var json = opts.JsonSerializerOptions;

                json.AllowTrailingCommas = true;
                json.NumberHandling |=
                    JsonNumberHandling.WriteAsString |
                    JsonNumberHandling.AllowNamedFloatingPointLiterals;
                json.ReadCommentHandling = JsonCommentHandling.Skip;
                json.WriteIndented = true;

                json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
            })
            .AddApplicationPart(typeof(ThisAssembly).Assembly)
            .Services
            .AddMarkdown(cfg =>
                cfg.ConfigureMarkdigPipeline = builder =>
                    builder
                        .DisableHtml()
                        .UseBootstrap())
            .AddProblemDetails()
            .AddResponseCompression(opts => opts.EnableForHttps = true)
            .Configure<BrotliCompressionProviderOptions>(opts => opts.Level = CompressionLevel.SmallestSize)
            .Configure<GzipCompressionProviderOptions>(opts => opts.Level = CompressionLevel.SmallestSize)
            .AddRouting(opts =>
            {
                opts.LowercaseUrls = true;
                opts.LowercaseQueryStrings = true;
            });
    }
}
