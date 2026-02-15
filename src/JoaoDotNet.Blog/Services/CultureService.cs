using System.Globalization;
using Microsoft.JSInterop;

namespace JoaoDotNet.Blog.Services;

public sealed class CultureService
{
    private const string StorageKey = "locale";
    private const string DefaultLocale = "pt-PT";
    private readonly IJSRuntime jsRuntime;

    public CultureService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public string CurrentCulture { get; private set; } = DefaultLocale;

    public bool IsPortuguese => CurrentCulture.StartsWith("pt", StringComparison.OrdinalIgnoreCase);

    public async Task InitializeAsync()
    {
        var storedCulture = await jsRuntime.InvokeAsync<string>("cultureManager.getCulture", StorageKey, DefaultLocale);
        ApplyCulture(storedCulture);
    }

    public async Task SetCultureAsync(string culture)
    {
        var normalizedCulture = NormalizeCulture(culture);
        await jsRuntime.InvokeVoidAsync("cultureManager.setCulture", StorageKey, normalizedCulture);
        ApplyCulture(normalizedCulture);
    }

    private void ApplyCulture(string culture)
    {
        var normalizedCulture = NormalizeCulture(culture);
        CurrentCulture = normalizedCulture;
        var cultureInfo = CultureInfo.GetCultureInfo(normalizedCulture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    private static string NormalizeCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return DefaultLocale;
        }

        return culture.StartsWith("pt", StringComparison.OrdinalIgnoreCase) ? "pt-PT" : "en";
    }
}
