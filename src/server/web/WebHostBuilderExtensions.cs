namespace Arise.Server.Web;

public static class WebHostBuilderExtensions
{
    public static IHostBuilder ConfigureWebServices(this IHostBuilder builder, Action<IApplicationBuilder> configure)
    {
        return builder
            .ConfigureWebHost(builder =>
                builder
                    .UseKestrel((ctx, opts) =>
                    {
                        opts.ConfigureEndpointDefaults(opts => opts.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);

                        _ = opts.Configure(ctx.Configuration.GetSection("Kestrel"), true);
                    })
                    .UseStaticWebAssets()
                    .Configure((ctx, builder) =>
                    {
                        static bool IsApi(HttpContext context)
                        {
                            return context.Request.Path.StartsWithSegments("/Api", StringComparison.OrdinalIgnoreCase);
                        }

                        configure(builder);

                        _ = builder
                            .UseWhen(static ctx => IsApi(ctx), app => app.UseExceptionHandler())
                            .UseWhen(
                                static ctx => !IsApi(ctx),
                                app =>
                                    app
                                        .UseExceptionHandler("/Home/Exception")
                                        .UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}"))
                            .UseResponseCompression()
                            .UseStaticFiles()
                            .UseRouting()
                            .UseAuthentication()
                            .UseAuthorization()
                            .UseEndpoints(eps => eps.MapDefaultControllerRoute());
                    }));
    }
}
