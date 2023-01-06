using Arise.Server.Web.Models.News;
using Arise.Server.Web.News;

namespace Arise.Server.Web.Controllers;

public sealed class NewsController : WebController
{
    private readonly IOptionsSnapshot<WebOptions> _options;

    private readonly NewsArticleProvider _provider;

    public NewsController(IOptionsSnapshot<WebOptions> options, NewsArticleProvider provider)
    {
        _options = options;
        _provider = provider;
    }

    public IActionResult Index([FromQuery][Range(0, int.MaxValue)] int page)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var size = _options.Value.NewsPageSize;

        return View(new NewsIndexModel
        {
            Articles = _provider.Articles.Skip(size * page).Take(size),
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
