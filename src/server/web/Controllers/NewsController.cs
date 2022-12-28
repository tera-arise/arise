using Arise.Server.Web.Models.News;
using Arise.Server.Web.News;

namespace Arise.Server.Web.Controllers;

public sealed class NewsController : WebController
{
    private readonly NewsArticleProvider _provider;

    public NewsController(NewsArticleProvider provider)
    {
        _provider = provider;
    }

    public IActionResult Index()
    {
        // TODO: Add configurable pagination.

        return View(new NewsIndexModel
        {
            Articles = _provider.Articles.Take(10),
        });
    }

    [Route("[controller]/{year}/{month}/{day}/{slug}")]
    public IActionResult View([FromRoute] int year, [FromRoute] int month, [FromRoute] int day, [FromRoute] string slug)
    {
        LocalDate date;

        try
        {
            date = new(year, month, day);
        }
        catch (ArgumentOutOfRangeException)
        {
            return NotFound();
        }

        return _provider.Articles.SingleOrDefault(
            article => (article.Date, article.Slug) == (date, slug)) is { } article
            ? View(new NewsViewModel
            {
                Article = article,
            })
            : NotFound();
    }
}
