namespace Arise.Server.Web.Models.Api.News;

public sealed class NewsListResponse
{
    public sealed class NewsListResponseArticle
    {
        public required LocalDate Date { get; init; }

        public required string Slug { get; init; }

        public required string Title { get; init; }

        public required string Summary { get; init; }
    }

    public required IEnumerable<NewsListResponseArticle> Articles { get; init; }
}
