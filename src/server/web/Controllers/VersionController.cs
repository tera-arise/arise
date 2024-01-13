using Arise.Server.Web.Net;

namespace Arise.Server.Web.Controllers;

internal sealed class VersionController : ApiController
{
    [AllowAnonymous]
    [DisableRateLimiting]
    [HttpGet]
    public IActionResult Check(GameDownloadLinks provider)
    {
        return Ok(new VersionCheckResponse
        {
            TeraManifestUri = provider.TeraManifestUri,
            TeraDownloadFormat = provider.TeraDownloadUri,
            AriseManifestUri = provider.AriseManifestUri,
            AriseDownloadFormat = provider.AriseDownloadUri,
        });
    }
}
