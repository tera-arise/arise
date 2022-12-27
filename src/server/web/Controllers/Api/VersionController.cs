using Arise.Server.Web.Models.Api.Version;
using Arise.Server.Web.Services;

namespace Arise.Server.Web.Controllers.Api;

public sealed class VersionController : ApiController
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Check(GameDownloadProvider provider)
    {
        return Ok(new VersionCheckResponse
        {
            ClientManifestUri = provider.ClientManifestUri,
            ClientDownloadFormat = provider.ClientDownloadUri,
            AriseManifestUri = provider.AriseManifestUri,
            AriseDownloadFormat = provider.AriseDownloadUri,
        });
    }
}
