# Copilot Instructions

- Target .NET 10 and Blazor WebAssembly.
- Default culture is pt-PT, fallback language is en.
- UI strings must use resx resources under src/JoaoDotNet.Blog/Resources.
- Blog posts live in src/JoaoDotNet.Blog/wwwroot/posts.
- Update posts-index.pt.json and posts-index.en.json by running the tools
  project after changing Markdown posts.
- Keep client code free of build-only dependencies.
- Follow existing routing and localization patterns (localStorage + reload).
