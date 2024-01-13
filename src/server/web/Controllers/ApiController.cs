using Arise.Server.Web.Authentication;
using Arise.Server.Web.RateLimiting;

namespace Arise.Server.Web.Controllers;

[ApiController]
[Authorize(ApiAuthenticationHandler.Name)]
[Consumes(MediaTypeNames.Application.Json)]
[EnableRateLimiting(ApiRateLimiterPolicy.Name)]
[Produces(MediaTypeNames.Application.Json)]
[Route("[controller]/[action]")]
internal abstract class ApiController : ControllerBase
{
    [NonAction]
    public virtual StatusCodeResult Gone()
    {
        return new(StatusCodes.Status410Gone);
    }
}
