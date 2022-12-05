using Arise.Server.Web.Models.Home;
using Arise.Server.Web.Services;

namespace Arise.Server.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Download([FromServices] GameDownloadProvider provider)
    {
        return View(
            new HomeDownloadModel(
                provider.ClientManifestUri,
                provider.ClientDownloadUri,
                provider.AriseManifestUri,
                provider.AriseDownloadUri));
    }

    public IActionResult Error([FromQuery] int? code)
    {
        return View(new HomeErrorModel(code));
    }

    public IActionResult Exception()
    {
        return View(new HomeExceptionModel(HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error));
    }
}
