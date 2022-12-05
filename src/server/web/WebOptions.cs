namespace Arise.Server.Web;

public sealed class WebOptions : IOptions<WebOptions>
{
    public int ClientRevision { get; } = 387486;

    WebOptions IOptions<WebOptions>.Value => this;
}
