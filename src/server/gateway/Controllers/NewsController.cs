using Arise.Server.Gateway.News;

namespace Arise.Server.Gateway.Controllers;

[AllowAnonymous]
[DisableRateLimiting]
internal sealed class NewsController : ApiController
{
    [BindProperty]
    [FromServices]
    public required NewsArticleProvider ArticleProvider { get; init; }

    [HttpGet]
    public IActionResult List(NewsListRequest body, IOptions<GatewayOptions> options)
    {
        var size = options.Value.NewsPageSize;

        return Ok(new NewsListResponse
        {
            Articles = ArticleProvider.Articles
                .Skip(size * body.Page)
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

    [HttpGet]
    public IActionResult Get(NewsGetRequest body)
    {
        return ArticleProvider.Articles.SingleOrDefault(
            article => (article.Date, article.Slug) == (body.Date, body.Slug)) is { } article
            ? Ok(new NewsGetResponse
            {
                Title = article.Title,
                Summary = article.Summary,
                Content = article.Content,
            })
            : NotFound();
    }
}
