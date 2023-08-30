namespace Arise.Client;

internal sealed class SymbioteOptions : IOptions<SymbioteOptions>
{
    public bool Console { get; set; }

    public Uri WorldServerUri { get; set; } = new("arise://localhost:7801");

    SymbioteOptions IOptions<SymbioteOptions>.Value => this;

    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        _ = services
            .AddOptions<SymbioteOptions>()
            .BindConfiguration("Symbiote");
    }
}
