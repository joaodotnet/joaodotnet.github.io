namespace JoaoDotNet.Blog.Models;

public sealed record PostMetadata
{
    public string Title { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string[] Tags { get; init; } = Array.Empty<string>();
    public string Summary { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Lang { get; init; } = "pt";
    public int ReadTimeMinutes { get; init; }
    public bool HasTranslation { get; init; }
    public string TranslationSlug { get; init; } = string.Empty;
    public string CoverImage { get; init; } = string.Empty;
}
