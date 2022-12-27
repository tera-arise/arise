namespace Arise.Server.Web;

public sealed class WebOptions : IOptions<WebOptions>
{
    public int ClientRevision { get; set; } = 387486;

    public string? SendGridKey { get; set; }

    public string MailAddress { get; set; } = "system@tera-arise.io";

    public Duration AccountVerificationTime { get; set; } = Duration.FromDays(7);

    public Duration AccountRecoveryTime { get; set; } = Duration.FromDays(1);

    public Duration AccountDeletionTime { get; set; } = Duration.FromDays(31);

    public Duration AccountKeyTime { get; set; } = Duration.FromHours(12);

    WebOptions IOptions<WebOptions>.Value => this;
}
