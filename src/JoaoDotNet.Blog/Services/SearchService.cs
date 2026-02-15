using JoaoDotNet.Blog.Models;

namespace JoaoDotNet.Blog.Services;

public sealed class SearchService
{
    public IReadOnlyList<PostMetadata> FilterPosts(IReadOnlyList<PostMetadata> posts, string? query, string? tag)
    {
        IEnumerable<PostMetadata> filtered = posts;

        if (!string.IsNullOrWhiteSpace(tag))
        {
            filtered = filtered.Where(post => post.Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(post =>
                post.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                post.Summary.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                post.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        return filtered.ToList();
    }
}
