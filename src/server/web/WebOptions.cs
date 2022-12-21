namespace Arise.Server.Web;

public sealed class WebOptions : IOptions<WebOptions>
{
    public int ClientRevision { get; set; } = 387486;

    public string? SendGridKey { get; set; }

    WebOptions IOptions<WebOptions>.Value => this;
}
