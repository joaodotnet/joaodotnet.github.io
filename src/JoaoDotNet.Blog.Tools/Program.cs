using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

const int WordsPerMinute = 200;
const string DefaultBaseUrl = "https://joaodotnet.github.io";

var postsPath = ResolvePostsPath(args);
if (!Directory.Exists(postsPath))
{
	Console.Error.WriteLine($"Posts directory not found: {postsPath}");
	return 1;
}

var baseUrl = args.Length > 1 ? args[1] : DefaultBaseUrl;
var posts = LoadPosts(postsPath);
var grouped = posts.GroupBy(post => post.Slug, StringComparer.OrdinalIgnoreCase);

var indexPt = new List<PostIndexEntry>();
var indexEn = new List<PostIndexEntry>();
var sitemapEntries = new List<SitemapEntry>
{
	new("/", null),
	new("/posts", null),
	new("/about", null)
};

foreach (var group in grouped)
{
	var pt = group.FirstOrDefault(post => post.Lang == "pt");
	var en = group.FirstOrDefault(post => post.Lang == "en");

	if (pt is not null)
	{
		indexPt.Add(MapIndexEntry(pt, en is not null));
		sitemapEntries.Add(new($"/post/{pt.Slug}", pt.Date));
		sitemapEntries.Add(new($"/post/{pt.Slug}?lang=pt", pt.Date));
	}

	if (en is not null)
	{
		indexEn.Add(MapIndexEntry(en, pt is not null));
		sitemapEntries.Add(new($"/post/{en.Slug}", en.Date));
		sitemapEntries.Add(new($"/post/{en.Slug}?lang=en", en.Date));
	}
}

indexPt = indexPt.OrderByDescending(post => post.Date).ToList();
indexEn = indexEn.OrderByDescending(post => post.Date).ToList();

var jsonOptions = new JsonSerializerOptions
{
	PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	WriteIndented = true
};

await File.WriteAllTextAsync(Path.Combine(postsPath, "posts-index.pt.json"), JsonSerializer.Serialize(indexPt, jsonOptions));
await File.WriteAllTextAsync(Path.Combine(postsPath, "posts-index.en.json"), JsonSerializer.Serialize(indexEn, jsonOptions));

var sitemapContent = SitemapWriter.Write(baseUrl, sitemapEntries);
await File.WriteAllTextAsync(Path.Combine(postsPath, "..", "sitemap.xml"), sitemapContent);

Console.WriteLine("Post indexes and sitemap generated.");
return 0;

static string ResolvePostsPath(string[] args)
{
	if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
	{
		return Path.GetFullPath(args[0]);
	}

	return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "JoaoDotNet.Blog", "wwwroot", "posts"));
}

static List<PostSource> LoadPosts(string postsPath)
{
	var files = Directory.GetFiles(postsPath, "*.md", SearchOption.TopDirectoryOnly);
	var deserializer = new DeserializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance)
		.IgnoreUnmatchedProperties()
		.Build();

	var posts = new List<PostSource>();
	foreach (var file in files)
	{
		var fileName = Path.GetFileName(file);
		var language = fileName.Contains(".pt.", StringComparison.OrdinalIgnoreCase) ? "pt" : "en";
		var slug = fileName.Replace($".{language}.md", string.Empty, StringComparison.OrdinalIgnoreCase);
		var content = File.ReadAllText(file);
		var (frontMatter, body) = ParseFrontMatter(content, deserializer);

		var readTime = CalculateReadTime(body);
		posts.Add(new PostSource(
			Title: frontMatter.Title ?? slug,
			Date: frontMatter.Date ?? DateTime.UtcNow.Date,
			Tags: frontMatter.Tags ?? Array.Empty<string>(),
			Summary: frontMatter.Summary ?? string.Empty,
			Slug: frontMatter.Slug ?? slug,
			Lang: string.IsNullOrWhiteSpace(frontMatter.Lang) ? language : frontMatter.Lang,
			CoverImage: frontMatter.CoverImage ?? string.Empty,
			ReadTimeMinutes: readTime));
	}

	return posts;
}

static (PostFrontMatter FrontMatter, string Body) ParseFrontMatter(string content, IDeserializer deserializer)
{
	if (!content.TrimStart().StartsWith("---", StringComparison.Ordinal))
	{
		return (new PostFrontMatter(), content);
	}

	var normalized = content.TrimStart();
	var endIndex = normalized.IndexOf("---", 3, StringComparison.Ordinal);
	if (endIndex < 0)
	{
		return (new PostFrontMatter(), content);
	}

	var header = normalized[3..endIndex];
	var body = normalized[(endIndex + 3)..];
	var frontMatter = deserializer.Deserialize<PostFrontMatter>(header) ?? new PostFrontMatter();
	return (frontMatter, body);
}

static int CalculateReadTime(string content)
{
	var wordCount = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
	return Math.Max(1, (int)Math.Ceiling(wordCount / (double)WordsPerMinute));
}

static PostIndexEntry MapIndexEntry(PostSource post, bool hasTranslation)
{
	return new PostIndexEntry
	{
		Title = post.Title,
		Date = post.Date,
		Tags = post.Tags,
		Summary = post.Summary,
		Slug = post.Slug,
		Lang = post.Lang,
		ReadTimeMinutes = post.ReadTimeMinutes,
		HasTranslation = hasTranslation,
		TranslationSlug = post.Slug,
		CoverImage = post.CoverImage
	};
}

sealed record PostSource(
	string Title,
	DateTime Date,
	string[] Tags,
	string Summary,
	string Slug,
	string Lang,
	string CoverImage,
	int ReadTimeMinutes);

sealed class PostFrontMatter
{
	public string? Title { get; init; }
	public DateTime? Date { get; init; }
	public string[]? Tags { get; init; }
	public string? Summary { get; init; }
	public string? Slug { get; init; }
	public string? Lang { get; init; }
	public string? CoverImage { get; init; }
}

sealed class PostIndexEntry
{
	public string Title { get; init; } = string.Empty;
	public DateTime Date { get; init; }
	public string[] Tags { get; init; } = Array.Empty<string>();
	public string Summary { get; init; } = string.Empty;
	public string Slug { get; init; } = string.Empty;
	public string Lang { get; init; } = string.Empty;
	public int ReadTimeMinutes { get; init; }
	public bool HasTranslation { get; init; }
	public string TranslationSlug { get; init; } = string.Empty;
	public string CoverImage { get; init; } = string.Empty;
}

sealed record SitemapEntry(string Path, DateTime? LastModified);

static class SitemapWriter
{
	public static string Write(string baseUrl, IEnumerable<SitemapEntry> entries)
	{
		var lines = new List<string>
		{
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>",
			"<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">"
		};

		foreach (var entry in entries.DistinctBy(e => e.Path))
		{
			lines.Add("  <url>");
			lines.Add($"    <loc>{baseUrl.TrimEnd('/')}{entry.Path}</loc>");
			if (entry.LastModified is not null)
			{
				lines.Add($"    <lastmod>{entry.LastModified:yyyy-MM-dd}</lastmod>");
			}
			lines.Add("  </url>");
		}

		lines.Add("</urlset>");
		return string.Join(Environment.NewLine, lines);
	}
}
