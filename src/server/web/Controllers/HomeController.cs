using Arise.Server.Web.Models.Home;
using Arise.Server.Web.Services;

namespace Arise.Server.Web.Controllers;

public sealed class HomeController : WebController
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Download(GameDownloadProvider provider)
    {
        return View(new HomeDownloadModel
        {
            ClientManifestUri = provider.ClientManifestUri,
            ClientDownloadUri = provider.ClientDownloadUri,
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
