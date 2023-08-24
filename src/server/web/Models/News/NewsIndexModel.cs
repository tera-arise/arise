using Arise.Server.Web.News;

namespace Arise.Server.Web.Models.News;

internal sealed class NewsIndexModel
{
    public required IEnumerable<NewsArticle> Articles { get; init; }
}
