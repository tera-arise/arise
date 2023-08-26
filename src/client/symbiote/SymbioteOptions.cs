namespace Arise.Client;

public sealed class SymbioteOptions : IOptions<SymbioteOptions>
{
    public string AccountName { get; set; } = string.Empty;

    public string SessionTicket { get; set; } = string.Empty;

    public Uri WorldServerUri { get; set; } = new("arise://localhost:7801");

    SymbioteOptions IOptions<SymbioteOptions>.Value => this;
}
