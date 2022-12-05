namespace Arise.Server.Storage.Documents;

public sealed class ArticleDocument : IDocument
{
    [Identity]
    public required string Slug { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public required string Content { get; set; }

    // TODO: https://github.com/JasperFx/marten/issues/416
    [DuplicateField(NotNull = false, IndexSortOrder = SortOrder.Desc)]
    public required Instant Time { get; set; }
}
