// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Server.Gateway.Net;

namespace Arise.Server.Gateway.Controllers;

[AllowAnonymous]
[DisableRateLimiting]
internal sealed class LauncherController : ApiController
{
    [HttpGet]
    public IActionResult Hello(IOptions<GatewayOptions> options, GameDownloadLinks links)
    {
        var value = options.Value;

        return Ok(new LauncherHelloResponse
        {
            NewsUri = value.NewsUri is { } uri ? new(uri) : null,
            TeraManifestUri = links.TeraManifestUri,
            TeraDownloadFormat = links.TeraDownloadUri,
            AriseManifestUri = links.AriseManifestUri,
            AriseDownloadFormat = links.AriseDownloadUri,
            AccountDeletionTime = value.AccountDeletionTime,
        });
    }
}
