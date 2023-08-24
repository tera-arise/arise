using Arise.Server.Web.Models.Api.Version;
using Arise.Server.Web.Net;

namespace Arise.Server.Web.Controllers.Api;

internal sealed class VersionController : ApiController
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Check(GameDownloadLinks provider)
    {
        return Ok(new VersionCheckResponse
        {
            ClientManifestUri = provider.TeraManifestUri,
            ClientDownloadFormat = provider.TeraDownloadUri,
            AriseManifestUri = provider.AriseManifestUri,
            AriseDownloadFormat = provider.AriseDownloadUri,
        });
    }
}
