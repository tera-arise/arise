namespace Arise.Server.Web.Controllers;

public abstract class WebController : Controller
{
    [BindProperty]
    public required CancellationToken CancellationToken { get; init; }
}
