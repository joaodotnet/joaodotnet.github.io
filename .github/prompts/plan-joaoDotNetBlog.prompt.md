# Plan: Blazor WASM Personal Blog — joaodotnet.github.io (with i18n)

**TL;DR**: Build a standalone Blazor WebAssembly (.NET 10) static blog deployed to GitHub Pages at `joaodotnet.github.io`. Portuguese (pt-PT) is the default language, English (en) the secondary. Blog posts are bilingual Markdown files (`{slug}.pt.md` / `{slug}.en.md`). UI strings use `.resx` resource files with `IStringLocalizer`. Language is persisted in `localStorage` with a full app reload on switch. The design uses a bold red/black palette with .NET purple (`#512bd4`) accents.

---

## Steps

### 1. Scaffold the Blazor WASM Standalone Project

- Create solution `JoaoDotNet.Blog.sln` with a standalone Blazor WASM project targeting `net10.0`
- Add NuGet packages: **Markdig** (Markdown→HTML), **YamlDotNet** (front matter parsing), **Microsoft.Extensions.Localization** (i18n)
- Add a second project `JoaoDotNet.Blog.Tools` (console app) for the build-time post index generator
- Configure `.gitattributes` with `*.js binary` to prevent integrity check failures
- Call `builder.Services.AddLocalization()` in `Program.cs`

### 2. Project Structure

```
joaodotnet.github.io/
├── .github/workflows/deploy.yml
├── .gitattributes
├── JoaoDotNet.Blog.sln
├── src/
│   ├── JoaoDotNet.Blog/
│   │   ├── JoaoDotNet.Blog.csproj
│   │   ├── Program.cs                         # Read locale from localStorage, set CultureInfo
│   │   ├── App.razor
│   │   ├── Routes.razor
│   │   ├── Resources/
│   │   │   ├── SharedResources.resx            # pt-PT (default): Nav, footer, labels, buttons
│   │   │   ├── SharedResources.en.resx         # English translations
│   │   │   ├── Pages/
│   │   │   │   ├── Home.resx                   # pt-PT: Home page strings
│   │   │   │   ├── Home.en.resx
│   │   │   │   ├── About.resx
│   │   │   │   ├── About.en.resx
│   │   │   │   ├── Posts.resx
│   │   │   │   └── Posts.en.resx
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   └── NavMenu.razor                   # Inject IStringLocalizer<SharedResources>
│   │   ├── Pages/
│   │   │   ├── Home.razor                      # Landing: hero + latest 3 posts
│   │   │   ├── About.razor                     # Full bio with timeline
│   │   │   ├── Posts.razor                     # All posts + search/filter
│   │   │   └── Post.razor                      # Single post view (/post/{slug})
│   │   ├── Components/
│   │   │   ├── PostCard.razor
│   │   │   ├── SearchBar.razor
│   │   │   ├── TagBadge.razor
│   │   │   ├── MarkdownRenderer.razor
│   │   │   ├── HeroSection.razor
│   │   │   ├── ExperienceTimeline.razor
│   │   │   ├── LanguageToggle.razor            # Language switch button (PT | EN)
│   │   │   └── LoadingSpinner.razor
│   │   ├── Models/
│   │   │   └── PostMetadata.cs                 # + Lang field
│   │   ├── Services/
│   │   │   ├── BlogService.cs                  # Fetches locale-specific index + .md files
│   │   │   ├── SearchService.cs
│   │   │   └── CultureService.cs               # Read/write locale to localStorage
│   │   └── wwwroot/
│   │       ├── index.html                      # + JS: read localStorage locale, set <html lang>
│   │       ├── 404.html
│   │       ├── .nojekyll
│   │       ├── css/app.css
│   │       ├── images/profile.jpg
│   │       ├── posts/
│   │       │   ├── posts-index.pt.json         # Portuguese posts metadata
│   │       │   ├── posts-index.en.json         # English posts metadata
│   │       │   ├── welcome.pt.md               # Bilingual post pair
│   │       │   ├── welcome.en.md
│   │       │   └── ...
│   │       ├── robots.txt
│   │       └── sitemap.xml
│   └── JoaoDotNet.Blog.Tools/
│       ├── JoaoDotNet.Blog.Tools.csproj
│       └── Program.cs                          # Scans *.pt.md + *.en.md → separate index files
```

### 3. Localization Strategy — localStorage + Reload

This is the simplest approach that avoids route pollution and complex URL-based locale handling.

**Startup flow** (`Program.cs`):
1. Use JS interop to read `"locale"` from `localStorage` (default: `"pt-PT"`)
2. Set `CultureInfo.DefaultThreadCurrentCulture` and `DefaultThreadCurrentUICulture` **before** `builder.Build().RunAsync()`
3. .NET's ICU globalization data in WASM covers both `pt-PT` and `en` — dates, numbers format automatically

**Language toggle** (`LanguageToggle.razor`):
1. Display a toggle button in the nav bar showing `PT | EN` (active language highlighted)
2. On click: write new locale to `localStorage` via JS interop
3. Call `NavigationManager.NavigateTo(currentUrl, forceLoad: true)` to trigger a full reload with the new culture
4. Small JS snippet in `index.html` sets `<html lang="pt">` or `<html lang="en">` immediately on page load (before Blazor hydrates) by reading `localStorage`

**Why not URL-based (`/pt/about`, `/en/about`)?** It adds significant routing complexity (every page needs a `{lang}` param, every link must be locale-aware). For a personal site, bilingual SEO is low priority — your audience reaches you via LinkedIn/GitHub, not Google searches. If needed later, migration to URL-based is straightforward without changing `.resx` files or post structure.

### 4. UI Strings with `.resx` Resource Files

- **`SharedResources.resx`** (pt-PT, default) — ~20 keys for common UI:
  - `Nav_Home` = "Início", `Nav_Blog` = "Blog", `Nav_About` = "Sobre Mim"
  - `Footer_Rights` = "Todos os direitos reservados"
  - `Search_Placeholder` = "Pesquisar artigos..."
  - `ReadMore` = "Ler mais", `ReadTime` = "min de leitura"
  - `LatestPosts` = "Últimos Artigos", `AllPosts` = "Todos os Artigos"
  - `Language` = "Idioma", etc.

- **`SharedResources.en.resx`** — English equivalents:
  - `Nav_Home` = "Home", `Nav_About` = "About Me"
  - `Search_Placeholder` = "Search posts..."
  - `ReadMore` = "Read more", `ReadTime` = "min read"
  - etc.

- **Page-specific `.resx`** files for longer content blocks (About page bio sections, Home page hero text)

- Inject `IStringLocalizer<SharedResources>` in layouts/components. Usage: `@Localizer["Nav_Home"]`

- **Date formatting is automatic**: once `CultureInfo` is set, `@post.Date.ToString("D")` yields "15 de fevereiro de 2026" (pt-PT) or "February 15, 2026" (en)

### 5. Bilingual Blog Posts

- **Naming convention**: `{slug}.{lang}.md` — e.g., `welcome.pt.md` and `welcome.en.md`
- Each file has YAML front matter with a `lang` field:
  ```yaml
  ---
  title: "Bem-vindo ao Meu Blog"
  date: 2026-02-15
  tags: [blazor, dotnet, webassembly]
  summary: "Como construí um blog estático com Blazor WebAssembly"
  slug: welcome
  lang: pt
  coverImage: /images/posts/welcome.jpg
  ---
  ```
- Posts don't need to have both translations — a post can exist only in Portuguese. The UI gracefully handles missing translations (show a "Not available in this language" message with a link to the original)

**Post index — separate files per language:**
- `posts-index.pt.json` — metadata for all `*.pt.md` posts
- `posts-index.en.json` — metadata for all `*.en.md` posts
- `BlogService` fetches the index matching the current culture
- Each index entry includes a `translationAvailable: true/false` flag and a reference to the counterpart slug

### 6. Build-Time Post Index Generator (Updated)

`JoaoDotNet.Blog.Tools` now:
1. Scans `wwwroot/posts/*.pt.md` and `wwwroot/posts/*.en.md`
2. Groups by slug, detects which translations exist
3. Generates two separate index files: `posts-index.pt.json` and `posts-index.en.json`
4. Each entry includes: `title`, `date`, `tags`, `summary`, `slug`, `lang`, `readTimeMinutes`, `hasTranslation`
5. Also generates `sitemap.xml` including both language versions

### 7. BlogService (Updated)

- On startup, fetches `posts/posts-index.{currentLang}.json` via `HttpClient`
- When culture changes (language toggle), the app reloads and fetches the correct index
- On demand, fetches `posts/{slug}.{currentLang}.md`
- If a translated post doesn't exist, falls back to the default language (pt) and shows a notice

### 8. About Page — Bilingual Content

For the About page, since the content is rich and structured (bio, timeline, certifications), two approaches work together:

- **Static structured data** (experience, certifications, education) — stored in page-specific `.resx` files:
  - `About.resx` (pt-PT): job titles, descriptions, section headers in Portuguese
  - `About.en.resx`: same in English (content comes from your LinkedIn which is already in English)

- **Page sections**: Hero with name/title, bio summary, experience timeline, certifications grid, skills/technologies, languages, education, contact links. All text pulled from `IStringLocalizer<About>`

### 9. Pages Implementation

| Page | Route | Localized Content |
|---|---|---|
| **Home** | `/` | Hero text, "Latest Posts" heading, CTA buttons — all from `.resx`. Post cards from locale-specific index. |
| **About** | `/about` | Full bio, experience timeline, certifications, skills — all from page `.resx`. Dates formatted per locale. |
| **Posts** | `/posts` | Search placeholder, "All Posts" heading, tag labels — from `.resx`. Posts from locale-specific index. |
| **Post** | `/post/{slug}` | Markdown content from `{slug}.{lang}.md`. If translation missing, show fallback notice. Header labels from `.resx`. |

### 10. Design System — "Crimson Architect"

A bold, editorial design that mixes **red/black drama** with **.NET purple** accents:

- **Color palette** (CSS variables):
  - `--primary`: `#DC2626` (vivid red)
  - `--primary-dark`: `#991B1B` (deep red)
  - `--surface`: `#0A0A0A` (near-black background)
  - `--surface-elevated`: `#1A1A1A` (card surfaces)
  - `--accent`: `#512BD4` (.NET purple)
  - `--accent-light`: `#7B5EE0` (lighter purple for hovers)
  - `--text-primary`: `#F5F5F5` (off-white text)
  - `--text-secondary`: `#A3A3A3` (muted text)
  - `--border`: `#2A2A2A`

- **Typography**: Display font — **"Clash Display"** or **"Cabinet Grotesk"** (via Fontsource/CDN). Body font — **"Satoshi"** or **"General Sans"**. Monospace for code — **"JetBrains Mono"**.

- **Layout**: Dark theme, full-bleed hero with a dramatic red gradient slash. Cards with subtle red border glow on hover. Purple accent on interactive elements (links, active states, tag badges). Generous whitespace, asymmetric grid on landing page. Sticky nav with glassmorphism effect (`backdrop-filter: blur()`).

- **Motion**: Page transitions with staggered fade-in. Cards with subtle scale on hover. Smooth scroll-triggered reveals for the experience timeline. Loading screen with animated .NET logo.

- **Code blocks**: Dark syntax highlighting theme (One Dark / Dracula variant) with red accent line numbers and purple keywords.

### 11. GitHub Pages Deployment

- Repository: `joaodotnet.github.io` (user site, deploys to root)
- Base href: `/` (no rewrite needed for user site)
- GitHub Actions workflow (`.github/workflows/deploy.yml`):
  1. Checkout code
  2. Setup .NET 10 SDK
  3. Run `JoaoDotNet.Blog.Tools` to generate `posts-index.pt.json`, `posts-index.en.json`, and `sitemap.xml`
  4. `dotnet publish -c Release -o publish`
  5. Add `.nojekyll` to `publish/wwwroot/`
  6. Upload artifact with `actions/upload-pages-artifact`
  7. Deploy with `actions/deploy-pages`
- Include `404.html` with SPA redirect script (preserves deep links like `/post/my-article`)
- Include companion script in `index.html` `<head>` to restore URL from query params

### 12. SEO for Bilingual Site

- `<html lang>` set dynamically via JS on page load from `localStorage`
- `<noscript>` block in `index.html` with bilingual key info (Portuguese first, then English)
- `sitemap.xml` includes entries for both languages
- Open Graph meta tags set dynamically per page/locale via JS interop
- Accept the trade-off: shared URLs mean crawlers see only one language. Since pt-PT is default and your primary audience is Portuguese-speaking, this is fine

### 13. Starter Content

- 1-2 sample posts in both languages:
  - `welcome.pt.md` / `welcome.en.md` — "Bem-vindo ao Meu Blog" / "Welcome to My Blog"
  - `blazor-wasm-blog.pt.md` / `blazor-wasm-blog.en.md` — "Construindo Este Blog com Blazor" / "Building This Blog with Blazor WASM"
- About page populated from your LinkedIn data in both languages

---

## Verification

- Run locally with `dotnet run` — verify all pages render in Portuguese by default
- Toggle to English — confirm full reload, all UI strings change, posts list shows English posts, dates format correctly ("15 de fevereiro de 2026" → "February 15, 2026")
- Navigate to a post that exists only in Portuguese while in English mode — confirm fallback notice
- Refresh the page — confirm `localStorage` persists the language choice
- Inspect `<html lang>` attribute — confirm it matches the selected locale
- Push to repo — confirm GitHub Actions generates both index files and deploys successfully
- Lighthouse audit: check performance and accessibility

---

## Key Decisions

| Decision | Choice | Rationale |
|---|---|---|
| **Technology** | Pure Blazor WASM (.NET 10) | Full C# experience, interactive, .NET identity story |
| **Locale strategy** | localStorage + reload | Simplest; no route pollution; sufficient for personal site |
| **Resource format** | `.resx` files | Built-in `IStringLocalizer` support; compile-time safety |
| **Blog posts** | `{slug}.{lang}.md` with front matter | Flat structure; easy pairing; front matter carries metadata |
| **Post index** | Separate `posts-index.pt.json` / `posts-index.en.json` | Simple consumption; smaller payload per request |
| **URL routing** | No locale prefix in URLs | Matches localStorage approach; keeps routing simple |
| **Search** | LINQ over in-memory metadata | Simple, no JS interop, sufficient for a personal blog |
| **Markdown** | Markdig + YamlDotNet | Maximum control, minimal dependencies, actively maintained |
| **Design** | Dark theme: red/black + .NET purple | Matches user color preferences; bold and memorable |
| **Hosting** | GitHub Pages (user site) | Root URL deployment, no base path rewriting needed |
| **Post index generation** | Build-time via console tool | No manual JSON maintenance; adding a post = adding a `.md` file |
| **SEO trade-off** | Accept limited bilingual SEO | Audience via LinkedIn/GitHub, not Google; mitigated with meta tags and sitemap |

---

## Trade-offs Acknowledged

1. **Blazor WASM vs Hugo**: Slower initial load (~4MB WASM runtime), weaker SEO — but full C# interactivity and .NET identity
2. **localStorage vs URL-based locale**: Shared URLs per language means crawlers see only default language — acceptable for personal site
3. **Posts can be monolingual**: Not every post needs both translations — graceful fallback to default language (pt-PT) with notice
