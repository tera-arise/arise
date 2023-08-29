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

                        static bool IsApi(HttpContext context)
                        {
                            return context.Request.Path.StartsWithSegments("/Api", StringComparison.OrdinalIgnoreCase);
                        }

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
                            .UseWhen(static ctx => IsApi(ctx), static app => app.UseExceptionHandler())
                            .UseWhen(
                                static ctx => !IsApi(ctx),
                                static app =>
                                    app
                                        .UseExceptionHandler("/Home/Exception")
                                        .UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}"))
                            .UseStaticFiles()
                            .UseRouting()
                            .UseRateLimiter()
                            .UseAuthentication()
                            .UseAuthorization()
                            .UseEndpoints(static eps => eps.MapDefaultControllerRoute());
                    }));
    }
}
