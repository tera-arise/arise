using Arise.Server.Web.Models.Articles;

namespace Arise.Server.Web.Controllers;

public sealed class ArticlesController : Controller
{
    private readonly IQuerySession _session;

    public ArticlesController(IQuerySession session)
    {
        _session = session;
    }

    public async Task<IActionResult> IndexAsync(CancellationToken cancellationToken)
    {
        return View(
            new ArticlesIndexModel(
                await _session
                    .Query<ArticleDocument>()
                    .OrderByDescending(article => article.Time)
                    .Take(10)
                    .ToListAsync(cancellationToken)));
    }

    public async Task<IActionResult> ViewAsync([FromRoute] string id, CancellationToken cancellationToken)
    {
        return await _session
            .Query<ArticleDocument>()
            .SingleOrDefaultAsync(article => article.Slug == id, cancellationToken) is ArticleDocument article
                ? View(new ArticlesViewModel(article))
                : NotFound();
    }
}
