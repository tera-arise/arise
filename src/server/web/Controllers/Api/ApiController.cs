using Arise.Server.Web.Authentication;

namespace Arise.Server.Web.Controllers.Api;

[ApiController]
[Authorize(Policy = ApiAuthenticationHandler.Name)]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]/[action]")]
internal abstract class ApiController : ControllerBase
{
    [BindProperty]
    public required CancellationToken CancellationToken { get; init; }

    [NonAction]
    public virtual StatusCodeResult Gone()
    {
        return new(StatusCodes.Status410Gone);
    }
}
