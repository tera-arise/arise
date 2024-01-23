using Arise.Server.Gateway.Net;

namespace Arise.Server.Gateway.Controllers;

[AllowAnonymous]
[DisableRateLimiting]
internal sealed class LauncherController : ApiController
{
    [HttpGet]
    public IActionResult Hello(IOptions<GatewayOptions> options, GameDownloadLinks links)
    {
        return Ok(new LauncherHelloResponse
        {
            NewsUri = options.Value.NewsUri is { } uri ? new(uri) : null,
            TeraManifestUri = links.TeraManifestUri,
            TeraDownloadFormat = links.TeraDownloadUri,
            AriseManifestUri = links.AriseManifestUri,
            AriseDownloadFormat = links.AriseDownloadUri,
        });
    }
}
