using Arise.Server.Web.Models.Home;
using Arise.Server.Web.Net;

namespace Arise.Server.Web.Controllers;

internal sealed class HomeController : WebController
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Download(GameDownloadLinks provider)
    {
        return View(new HomeDownloadModel
        {
            TeraManifestUri = provider.TeraManifestUri,
            TeraDownloadUri = provider.TeraDownloadUri,
            AriseManifestUri = provider.AriseManifestUri,
            AriseDownloadUri = provider.AriseDownloadUri,
        });
    }

    public IActionResult Error([FromQuery] int? code)
    {
        return View(new HomeErrorModel
        {
            Code = code,
        });
    }

    public IActionResult Exception()
    {
        return View(new HomeExceptionModel
        {
            Exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error,
        });
    }
}
