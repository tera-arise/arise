using Arise.Server.Gateway.Net;

namespace Arise.Server.Gateway.Controllers;

[AllowAnonymous]
[DisableRateLimiting]
internal sealed class VersionController : ApiController
{
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
