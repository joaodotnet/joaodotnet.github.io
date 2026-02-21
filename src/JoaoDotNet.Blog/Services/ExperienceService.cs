using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
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

        var loadedExperiences = await TryLoadExperiencesAsync(cultureCode);

        if (loadedExperiences.Count == 0 && !string.Equals(cultureCode, "en", StringComparison.OrdinalIgnoreCase))
        {
            loadedExperiences = await TryLoadExperiencesAsync("en");
        }

        if (loadedExperiences.Count == 0 && !string.Equals(cultureCode, "pt", StringComparison.OrdinalIgnoreCase))
        {
            loadedExperiences = await TryLoadExperiencesAsync("pt");
        }

        cache[cultureCode] = loadedExperiences;
        return loadedExperiences;
    }

    private async Task<List<TimelineEntry>> TryLoadExperiencesAsync(string cultureCode)
    {
        try
        {
            var indexPath = $"experience/experience-index.{cultureCode}.json";
            return await httpClient.GetFromJsonAsync<List<TimelineEntry>>(indexPath) ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (NotSupportedException)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
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
