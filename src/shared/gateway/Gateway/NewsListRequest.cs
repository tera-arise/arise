namespace Arise.Gateway;

public sealed class NewsListRequest
{
    [Range(0, int.MaxValue)]
    public required int Page { get; set; }
}
