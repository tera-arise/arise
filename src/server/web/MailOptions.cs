namespace Arise.Server.Web;

public sealed class MailOptions : IOptions<MailOptions>
{
    public string? SendGridKey { get; set; }

    MailOptions IOptions<MailOptions>.Value => this;
}
