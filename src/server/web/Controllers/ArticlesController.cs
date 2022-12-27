using Arise.Server.Web.Models.Articles;

namespace Arise.Server.Web.Controllers;

public sealed class ArticlesController : WebController
{
    [BindProperty]
    [FromServices]
    public required IDocumentStore Store { get; init; }

    public async ValueTask<IActionResult> IndexAsync()
    {
        await using var session = Store.QuerySession();

        return View(new ArticlesIndexModel
        {
            Articles = await session
                .Query<ArticleDocument>()
                .OrderByDescending(article => article.Time)
                .Take(10)
                .ToListAsync(CancellationToken),
        });
    }

    public async ValueTask<IActionResult> ViewAsync([FromRoute] string id)
    {
        if (!ModelState.IsValid)
            return NotFound();

        await using var session = Store.QuerySession();

        return await session
            .Query<ArticleDocument>()
            .SingleOrDefaultAsync(article => article.Slug == id, CancellationToken) is ArticleDocument article
            ? View(new ArticlesViewModel
            {
                Article = article,
            })
            : NotFound();
    }
}
