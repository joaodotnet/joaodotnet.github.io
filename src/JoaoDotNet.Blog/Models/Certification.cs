namespace JoaoDotNet.Blog.Models;

public sealed record Certification
{
    public string Name { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string CredentialUrl { get; init; } = string.Empty;
}
