namespace Arise.Server.Web.Controllers.Api;

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]/[action]")]
public abstract class ApiController : ControllerBase
{
}
