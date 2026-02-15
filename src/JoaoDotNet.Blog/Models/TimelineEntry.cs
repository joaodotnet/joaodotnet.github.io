namespace JoaoDotNet.Blog.Models;

public sealed record TimelineEntry
{
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
