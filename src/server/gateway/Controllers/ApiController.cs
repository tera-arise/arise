// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Server.Gateway.Authentication;
using Arise.Server.Gateway.RateLimiting;

namespace Arise.Server.Gateway.Controllers;

[ApiController]
[Authorize(ApiAuthenticationHandler.Name)]
[Consumes(MediaTypeNames.Application.Json)]
[EnableRateLimiting(ApiRateLimiterPolicy.Name)]
[Produces(MediaTypeNames.Application.Json)]
[Route("[controller]/[action]")]
internal abstract class ApiController : ControllerBase
{
    [NonAction]
    public new virtual StatusCodeResult Forbid()
    {
        return new(StatusCodes.Status403Forbidden);
    }
}
