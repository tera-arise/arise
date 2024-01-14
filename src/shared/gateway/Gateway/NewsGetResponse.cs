namespace Arise.Gateway;

public sealed class NewsGetResponse
{
    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required string Content { get; init; }
}
