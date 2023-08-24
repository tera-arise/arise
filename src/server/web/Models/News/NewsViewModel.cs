using Arise.Server.Web.News;

namespace Arise.Server.Web.Models.News;

internal sealed class NewsViewModel
{
    public required NewsArticle Article { get; init; }
}
