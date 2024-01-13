namespace Arise.Server.Web.RateLimiting;

internal sealed class ApiRateLimiterPolicy : IRateLimiterPolicy<IPAddress>
{
    public const string Name = "Api";

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    private readonly IOptions<WebOptions> _options;

    public ApiRateLimiterPolicy(IOptions<WebOptions> options)
    {
        _options = options;
    }

    public RateLimitPartition<IPAddress> GetPartition(HttpContext httpContext)
    {
        var ip = httpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
        var value = _options.Value;

        return IPAddress.IsLoopback(ip)
            ? RateLimitPartition.GetNoLimiter(ip)
            : RateLimitPartition.GetFixedWindowLimiter(
                ip,
                _ => new()
                {
                    QueueLimit = 0,
                    PermitLimit = value.RateLimit,
                    Window = value.RateLimitPeriod.ToTimeSpan(),
                });
    }
}
