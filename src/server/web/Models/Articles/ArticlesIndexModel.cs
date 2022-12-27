namespace Arise.Server.Web.Models.Articles;

public sealed class ArticlesIndexModel
{
    public required IEnumerable<ArticleDocument> Articles { get; init; }
}
