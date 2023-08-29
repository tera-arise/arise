namespace Arise.Server.Web;

public sealed class WebOptions : IOptions<WebOptions>
{
    public string ForwardedForHeader { get; set; } = "X-Forwarded-For";

    public ICollection<string> ForwardingProxyRanges { get; } = new List<string>();

    public int ApiRateLimit { get; set; } = 10;

    public Duration ApiRateLimitPeriod { get; set; } = Duration.FromHours(1);

    public int TeraRevision { get; set; } = 387486;

    public string TeraDownloadFormat { get; set; } =
        "https://github.com/tera-arise/arise-client/releases/r{0}/download/{1}";

    public string AriseDownloadFormat { get; set; } =
        "https://github.com/tera-arise/arise-release/releases/r{0}/download/{1}";

    public string? SendGridKey { get; set; }

    public string EmailAddress { get; set; } = "arise@localhost";

    public Duration AccountVerificationTime { get; set; } = Duration.FromDays(7);

    public Duration AccountRecoveryTime { get; set; } = Duration.FromDays(1);

    public Duration AccountDeletionTime { get; set; } = Duration.FromDays(31);

    public Duration AccountSessionKeyTime { get; set; } = Duration.FromHours(12);

    public int NewsPageSize { get; set; } = 10;

    WebOptions IOptions<WebOptions>.Value => this;
}
