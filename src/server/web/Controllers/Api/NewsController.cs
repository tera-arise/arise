using Arise.Server.Web.News;

namespace Arise.Server.Web.Controllers.Api;

internal sealed class NewsController : ApiController
{
    [AllowAnonymous]
    [DisableRateLimiting]
    [HttpGet]
    public IActionResult List(
        IOptions<WebOptions> options, NewsArticleProvider provider, [FromQuery][Range(0, int.MaxValue)] int page)
    {
        var size = options.Value.NewsPageSize;

        return Ok(new NewsListResponse
        {
            Articles = provider.Articles
                .Skip(size * page)
                .Take(size)
                .Select(static article => new NewsListResponse.NewsListResponseArticle
                {
                    Date = article.Date,
                    Slug = article.Slug,
                    Title = article.Title,
                    Summary = article.Summary,
                }),
        });
    }
}
