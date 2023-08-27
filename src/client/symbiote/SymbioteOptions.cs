namespace Arise.Client;

public sealed class SymbioteOptions : IOptions<SymbioteOptions>
{
    public Uri WorldServerUri { get; set; } = new("arise://localhost:7801");

    SymbioteOptions IOptions<SymbioteOptions>.Value => this;
}
