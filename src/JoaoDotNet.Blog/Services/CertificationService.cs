using System.Net.Http.Json;
using System.Text.Json;
using JoaoDotNet.Blog.Models;

namespace JoaoDotNet.Blog.Services;

public sealed class CertificationService
{
    private readonly HttpClient httpClient;
    private IReadOnlyList<Certification>? cache;

    public CertificationService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Certification>> GetCertificationsAsync()
    {
        if (cache is not null)
        {
            return cache;
        }

        cache = await TryLoadCertificationsAsync();
        return cache;
    }

    private async Task<List<Certification>> TryLoadCertificationsAsync()
    {
        try
        {
            var indexPath = "certifications/certifications-index.json";
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
}
