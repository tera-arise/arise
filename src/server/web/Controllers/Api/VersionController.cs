using Arise.Server.Web.Services;

namespace Arise.Server.Web.Controllers.Api;

public sealed class VersionController : ApiController
{
    public IActionResult Check([FromServices] GameDownloadProvider provider)
    {
        return Ok(new
        {
            ClientManifestUri = provider.ClientManifestUri,
            ClientDownloadFormat = provider.ClientDownloadUri,
            AriseManifestUri = provider.AriseManifestUri,
            AriseDownloadFormat = provider.AriseDownloadUri,
        });
    }
}
