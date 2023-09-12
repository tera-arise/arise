namespace Arise.Client;

internal sealed class SymbioteOptions : IOptions<SymbioteOptions>
{
    public bool Console { get; set; }

    public Uri WorldServerUri { get; set; } = new("arise://localhost:7801");

    public string AccountName { get; set; } = "arise@localhost";

    public string SessionTicket { get; set; } = "bc8d0fc43b795e0634fa5934a2d3b14d5348da9f13fa26ae57acefaf337f5731";

    SymbioteOptions IOptions<SymbioteOptions>.Value => this;

    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        _ = services
            .AddOptions<SymbioteOptions>()
            .BindConfiguration("Symbiote");
    }
}
