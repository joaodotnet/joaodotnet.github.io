# JoaoDotNet Blog

A bilingual Blazor WebAssembly (.NET 10) personal blog for joaodotnet.github.io.
Default language is Portuguese (pt-PT) with English as secondary. Blog posts are
Markdown files stored in wwwroot/posts.

## Why two projects

- JoaoDotNet.Blog: the Blazor WASM client app (UI, routing, localization, blog UI)
- JoaoDotNet.Blog.Tools: build-time console tool that scans Markdown posts and
  generates posts-index.pt.json, posts-index.en.json, and sitemap.xml

Keeping the tooling separate avoids shipping build-only dependencies to the
client and keeps the runtime payload smaller.

## Quick start

```bash
dotnet restore JoaoDotNet.Blog.sln

dotnet run --project src/JoaoDotNet.Blog.Tools --configuration Release

dotnet run --project src/JoaoDotNet.Blog
```


## Posts

- Files: wwwroot/posts/{slug}.pt.md and wwwroot/posts/{slug}.en.md
- Front matter supports: title, date, tags, summary, slug, lang, coverImage
- Add a post, then re-run the tools project to regenerate indexes

## Localization

- UI strings live in Resources/*.resx and Resources/Pages/*.resx
- Culture is stored in localStorage under "locale" and a full reload occurs on
  language switch

## Deployment

- GitHub Pages workflow lives in .github/workflows/deploy.yml
- CI runs the tools project before publish
