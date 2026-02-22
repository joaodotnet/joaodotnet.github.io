namespace JoaoDotNet.Blog.Models;

public sealed record Certification
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int? Order { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string CredentialUrl { get; init; } = string.Empty;
}
