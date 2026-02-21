using System.Globalization;
using System.Net.Http.Json;
using JoaoDotNet.Blog.Models;

namespace JoaoDotNet.Blog.Services;

public sealed class ExperienceService
{
    private readonly HttpClient httpClient;
    private readonly CultureService cultureService;
    private readonly Dictionary<string, IReadOnlyList<TimelineEntry>> cache = new(StringComparer.OrdinalIgnoreCase);

    public ExperienceService(HttpClient httpClient, CultureService cultureService)
    {
        this.httpClient = httpClient;
        this.cultureService = cultureService;
    }

    public async Task<IReadOnlyList<TimelineEntry>> GetExperiencesAsync()
    {
        var cultureCode = GetCultureCode();
        if (cache.TryGetValue(cultureCode, out var experiences))
        {
            return experiences;
        }

        var indexPath = $"experience/experience-index.{cultureCode}.json";
        var loadedExperiences = await httpClient.GetFromJsonAsync<List<TimelineEntry>>(indexPath) ?? new List<TimelineEntry>();
        cache[cultureCode] = loadedExperiences;
        return loadedExperiences;
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
}
