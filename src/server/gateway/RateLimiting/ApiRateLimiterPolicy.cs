namespace Arise.Server.Gateway.RateLimiting;

internal sealed class ApiRateLimiterPolicy : IRateLimiterPolicy<IPAddress>
{
    public const string Name = "Api";

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    private readonly IOptions<GatewayOptions> _options;

    public ApiRateLimiterPolicy(IOptions<GatewayOptions> options)
    {
        _options = options;
    }

    public RateLimitPartition<IPAddress> GetPartition(HttpContext httpContext)
    {
        var ip = httpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
        var options = _options.Value;

        return IPAddress.IsLoopback(ip)
            ? RateLimitPartition.GetNoLimiter(ip)
            : RateLimitPartition.GetFixedWindowLimiter(
                ip,
                _ => new()
                {
                    QueueLimit = 0,
                    PermitLimit = options.RateLimit,
                    Window = options.RateLimitPeriod.ToTimeSpan(),
                });
    }
}
