namespace Arise.Server.Web;

public static class WebHostBuilderExtensions
{
    public static IHostBuilder ConfigureWebServices(this IHostBuilder builder, Action<IApplicationBuilder> configure)
    {
        return builder
            .ConfigureWebHost(builder =>
                builder
                    .UseKestrel(static (ctx, opts) =>
                    {
                        opts.ConfigureEndpointDefaults(
                            static opts => opts.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);

                        _ = opts.Configure(ctx.Configuration.GetSection("Kestrel"));
                    })
                    .Configure((ctx, builder) =>
                    {
                        configure(builder);

                        var options = builder.ApplicationServices.GetRequiredService<IOptions<WebOptions>>().Value;
                        var fho = new ForwardedHeadersOptions
                        {
                            ForwardedForHeaderName = options.ForwardedForHeader,
                            ForwardedHeaders = ForwardedHeaders.XForwardedFor,
                        };

                        foreach (var range in options.ForwardingProxyRanges)
                            fho.KnownNetworks.Add(Microsoft.AspNetCore.HttpOverrides.IPNetwork.Parse(range));

                        _ = builder
                            .UseForwardedHeaders(fho)
                            .UseResponseCompression()
                            .UseExceptionHandler()
                            .UseRouting()
                            .UseRateLimiter()
                            .UseAuthentication()
                            .UseAuthorization()
                            .UseEndpoints(static eps => eps.MapControllers());
                    }));
    }
}
