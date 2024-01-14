namespace Arise.Gateway;

public sealed class NewsGetRequest
{
    public required LocalDate Date { get; init; }

    public required string Slug { get; init; }
}
