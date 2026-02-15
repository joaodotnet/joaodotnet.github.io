using System.Globalization;
using System.Net.Http.Json;
using JoaoDotNet.Blog.Models;

namespace JoaoDotNet.Blog.Services;

public sealed class BlogService
{
    private readonly HttpClient httpClient;
    private readonly CultureService cultureService;
    private readonly Dictionary<string, IReadOnlyList<PostMetadata>> cache = new(StringComparer.OrdinalIgnoreCase);

    public BlogService(HttpClient httpClient, CultureService cultureService)
    {
        this.httpClient = httpClient;
        this.cultureService = cultureService;
    }

    public async Task<IReadOnlyList<PostMetadata>> GetPostsAsync()
    {
        var cultureCode = GetCultureCode();
        if (cache.TryGetValue(cultureCode, out var posts))
        {
            return posts;
        }

        var indexPath = $"posts/posts-index.{cultureCode}.json";
        var loadedPosts = await httpClient.GetFromJsonAsync<List<PostMetadata>>(indexPath) ?? new List<PostMetadata>();
        cache[cultureCode] = loadedPosts;
        return loadedPosts;
    }

    public async Task<(PostMetadata? Metadata, string Content, bool IsFallback)> GetPostAsync(string slug)
    {
        var cultureCode = GetCultureCode();
        var isFallback = false;

        var posts = await GetPostsAsync();
        var metadata = posts.FirstOrDefault(post => string.Equals(post.Slug, slug, StringComparison.OrdinalIgnoreCase));
        var content = await TryGetMarkdownAsync(slug, cultureCode);

        if (content is null)
        {
            isFallback = true;
            cultureCode = "pt";
            content = await TryGetMarkdownAsync(slug, cultureCode) ?? string.Empty;
            var fallbackPosts = await GetPostsByCultureAsync(cultureCode);
            metadata = fallbackPosts.FirstOrDefault(post => string.Equals(post.Slug, slug, StringComparison.OrdinalIgnoreCase));
        }

        return (metadata, StripFrontMatter(content), isFallback);
    }

    public async Task<IReadOnlyList<PostMetadata>> GetPostsByCultureAsync(string cultureCode)
    {
        if (cache.TryGetValue(cultureCode, out var posts))
        {
            return posts;
        }

        var indexPath = $"posts/posts-index.{cultureCode}.json";
        var loadedPosts = await httpClient.GetFromJsonAsync<List<PostMetadata>>(indexPath) ?? new List<PostMetadata>();
        cache[cultureCode] = loadedPosts;
        return loadedPosts;
    }

    private async Task<string?> TryGetMarkdownAsync(string slug, string cultureCode)
    {
        var response = await httpClient.GetAsync($"posts/{slug}.{cultureCode}.md");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    private string GetCultureCode()
    {
        if (cultureService.IsPortuguese)
        {
            return "pt";
        }

        var cultureName = CultureInfo.CurrentUICulture.Name;
        return cultureName.StartsWith("pt", StringComparison.OrdinalIgnoreCase) ? "pt" : "en";
    }

    private static string StripFrontMatter(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || !content.StartsWith("---", StringComparison.Ordinal))
        {
            return content;
        }

        var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
        if (endIndex <= 0)
        {
            return content;
        }

        return content[(endIndex + 3)..].TrimStart();
    }
}
