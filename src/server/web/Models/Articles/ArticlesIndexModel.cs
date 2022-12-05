namespace Arise.Server.Web.Models.Articles;

public sealed record class ArticlesIndexModel(IEnumerable<ArticleDocument> Articles);
