using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using JoaoDotNet.Blog;
using JoaoDotNet.Blog.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<CultureService>();
builder.Services.AddScoped<ExperienceService>();
builder.Services.AddScoped<CertificationService>();

var host = builder.Build();

await host.Services.GetRequiredService<CultureService>().InitializeAsync();
CultureInfo.DefaultThreadCurrentCulture ??= CultureInfo.GetCultureInfo("pt-PT");
CultureInfo.DefaultThreadCurrentUICulture ??= CultureInfo.GetCultureInfo("pt-PT");

await host.RunAsync();
