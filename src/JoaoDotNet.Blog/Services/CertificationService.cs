using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using JoaoDotNet.Blog.Models;

namespace JoaoDotNet.Blog.Services;

public sealed class CertificationService
{
    private readonly HttpClient httpClient;
    private readonly CultureService cultureService;
    private readonly Dictionary<string, IReadOnlyList<Certification>> cache = new(StringComparer.OrdinalIgnoreCase);

    public CertificationService(HttpClient httpClient, CultureService cultureService)
    {
        this.httpClient = httpClient;
        this.cultureService = cultureService;
    }

    public async Task<IReadOnlyList<Certification>> GetCertificationsAsync()
    {
        var cultureCode = GetCultureCode();
        if (cache.TryGetValue(cultureCode, out var certifications))
        {
            return certifications;
        }

        var loadedCertifications = await TryLoadCertificationsAsync(cultureCode);

        if (loadedCertifications.Count == 0 && !string.Equals(cultureCode, "en", StringComparison.OrdinalIgnoreCase))
        {
            loadedCertifications = await TryLoadCertificationsAsync("en");
        }

        if (loadedCertifications.Count == 0 && !string.Equals(cultureCode, "pt", StringComparison.OrdinalIgnoreCase))
        {
            loadedCertifications = await TryLoadCertificationsAsync("pt");
        }

        cache[cultureCode] = loadedCertifications;
        return loadedCertifications;
    }

    private async Task<List<Certification>> TryLoadCertificationsAsync(string cultureCode)
    {
        try
        {
            var indexPath = $"certifications/certifications-index.{cultureCode}.json";
            return await httpClient.GetFromJsonAsync<List<Certification>>(indexPath) ?? [];
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
