// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Gateway;

internal sealed class GatewayOptions : IOptions<GatewayOptions>
{
    public string ForwardedForHeader { get; set; } = "X-Forwarded-For";

    public ICollection<string> ForwardingProxyRanges { get; } = [];

    public int RateLimit { get; set; } = 10;

    public TimeSpan RateLimitPeriod { get; set; } = TimeSpan.FromHours(1);

    public int TeraRevision { get; set; } = 387486;

    public string TeraDownloadFormat { get; set; } =
        "https://github.com/tera-arise/arise-client/releases/r{0}/download/{1}";

    public string AriseDownloadFormat { get; set; } =
        "https://github.com/tera-arise/arise-release/releases/r{0}/download/{1}";

    public string? SendGridKey { get; set; }

    public string SendGridRegion { get; set; } = "global";

    public string EmailAddress { get; set; } = "arise@localhost";

    public TimeSpan AccountVerificationTime { get; set; } = TimeSpan.FromDays(7);

    public TimeSpan AccountRecoveryTime { get; set; } = TimeSpan.FromDays(1);

    public TimeSpan AccountDeletionTime { get; set; } = TimeSpan.FromDays(31);

    public TimeSpan AccountAuthenticationTime { get; set; } = TimeSpan.FromHours(12);

    public string? NewsUri { get; set; }

    GatewayOptions IOptions<GatewayOptions>.Value => this;

    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        _ = services
            .AddOptions<GatewayOptions>()
            .BindConfiguration("Gateway");
    }
}
