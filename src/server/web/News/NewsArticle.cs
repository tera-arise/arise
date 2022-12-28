namespace Arise.Server.Web.News;

public sealed class NewsArticle
{
    public required LocalDate Date { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required HtmlString Content { get; init; }
}
