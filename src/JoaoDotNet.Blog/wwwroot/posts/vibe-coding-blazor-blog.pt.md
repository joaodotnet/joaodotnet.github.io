---
title: "Vibe Coding de um Blog em Blazor WASM: Zero Código Manual"
date: 2026-03-19
tags: [blazor, webassembly, ai, dotnet10]
summary: "Como construí este blog estático em Blazor WebAssembly do zero utilizando agentes de IA, Claude Opus 4.6 e GPT-5.3-codex, sem digitar uma única linha de código aplicacional."
slug: vibe-coding-blazor-blog
lang: pt
coverImage: /images/profile.jpg
---

**TL;DR:** Construí este blog estático usando Blazor WebAssembly (.NET 10) em vez do padrão da indústria, o Hugo. Toda a aplicação foi gerada artificialmente ("vibe coded") através de agentes de IA, usando uma mistura de Claude 4.6 Opus, GPT-5.3-codex e Gemini, baseando-se fortemente no Microsoft Docs MCP para assegurar conhecimento preciso em .NET 10. O site é implementado automaticamente no GitHub Pages.

*Este artigo assume alguma familiaridade com .NET, geradores de sites estáticos e assistentes de código com IA.*

## O Problema: Porquê não usar o Hugo?

Se queres construir um blog estático hoje em dia, as recomendações por defeito são Hugo, Next.js ou Astro. São rápidos, fiáveis e cumprem o objetivo.
    
Mas, sendo um programador primariamente focado em .NET, queria fazer algo diferente. Adoro Blazor e já o utilizei intensivamente em aplicações empresariais do lado do servidor (Server-Side). No entanto, ainda não tinha mergulhado a fundo nas capacidades do Blazor WebAssembly (WASM). Construir um blog estático cliente-side foi a desculpa perfeita para explorar o WASM no .NET 10, correndo inteiramente no browser.

## A Solução: Vibe Coding da Arquitetura

![Fluxo de arquitetura dos Agentes](/images/posts/vibe-coding-workflow.svg)

A parte mais interessante deste projeto não é apenas a tecnologia utilizada, é *como* foi construído. Eu não escrevi o código manualmente. Em vez disso, adotei um workflow chamado "vibe coding", recorrendo de forma massiva a agentes de Inteligência Artificial.

### Passo 1: A Fase de Planeamento

Comecei no modo "Plan" do Copilot. Utilizando o **Claude Opus 4.6**, estabeleci os meus requisitos fundamentais: um tema escuro (vermelho, preto, violeta .NET), blog bilingue (PT/EN), publicações baseadas em markdown e uma abordagem de localização dependente de local storage.

O agente explorou a workspace e gerou um ficheiro de especificação detalhado (`plan-joaoDotNetBlog.prompt.md`). Este documento de arquitetura definiu exatamente como a app Blazor, o parser de markdown (`Markdig`) e os ficheiros locais de tradução (`.resx`) iriam interagir.

### Passo 2: A Fase de Execução

Com o plano estruturado, forneci a prompt de volta ao agente, mas desta vez a executar com **GPT-5.2-codex** e **GPT-5.3-codex**. Visto que a IA por vezes demonstra dificuldades com integrações de bibliotecas complexas, forcei a utilização da ferramenta **Microsoft Docs MCP** (Model Context Protocol). Isto garantiu que o agente procurasse e validasse código em tempo real na documentação oficial de .NET 10 e Blazor MASM, evitando "alucinações".

Também contei com o auxílio do **Gemini** para o raciocínio arquitetural mais denso. Para além das ferramentas base, os agentes usaram bastante as **Skills** — nomeadamente as `frontend-skills` da Anthropic — para produzir de forma instantânea práticas sólidas de CSS e um design responsivo irrepreensível. E, para finalizar com polimento, o agente recorreu a *skills* locais no repositório, nomeadamente a *skill* `image-enhancer`, para optimizar visualmente elementos gerados sem que tivesse de abrir ferramentas de design adicionais.

Em baixo encontra-se um excerto gerado de como a aplicação processa o Markdown perfeitamente do lado do cliente com recurso ao Markdig:

```csharp
// Gerado pela IA: processamento do markdown do lado do cliente
var pipeline = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .UsePragmaLines()
    .Build();
    
var htmlContent = Markdown.ToHtml(rawMarkdownText, pipeline);
```

### Passo 3: Implementação (Onde repousa o código)

A página é alojada diretamente no GitHub Pages. Para evitar a necessidade de um backend dedicado, construí uma pequena aplicação de consola secundária (`JoaoDotNet.Blog.Tools`). Esta corre na fase de CI/CD para processar todo o metadata e traduzir os ficheiros de markdown num formato em JSON pre-indexado (`posts-index.json`).

Abaixo apresento o workflow exato de GitHub Actions que compila a App em Blazor, corre a ferramenta de índices e publica tudo nas Github Pages:

```yaml
# .github/workflows/deploy.yml
name: Deploy to GitHub Pages

on:
  push:
    branches:
      - main
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Restore
        run: dotnet restore JoaoDotNet.Blog.sln

      - name: Generate post index
        run: dotnet run --project src/JoaoDotNet.Blog.Tools --configuration Release

      - name: Publish
        run: dotnet publish src/JoaoDotNet.Blog/JoaoDotNet.Blog.csproj -c Release -o publish

      - name: Ensure .nojekyll
        run: touch publish/wwwroot/.nojekyll

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: publish/wwwroot

  deploy:
    runs-on: ubuntu-latest
    needs: build
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - name: Deploy
        id: deployment
        uses: actions/deploy-pages@v4
```

### Passo 4: Analítica com Umami

Para estatísticas básicas do site, quis evitar o peso e as preocupações de privacidade do Google Analytics. Em vez disso, integrei o **Umami**. É uma alternativa de código aberto, focada na privacidade, que é gratuita e incrivelmente fácil de configurar. Complementa na perfeição um site estático, necessitando apenas da injeção de uma pequena tag de script, dando-me exatamente as métricas que preciso sem comprometer a privacidade do utilizador.

## Compromissos e Limitações

Embora a solução seja incrivelmente produtiva, existem realidades que tive de aceitar:

1. **Peso da Carga Inicial (Payload):** Ao contrário do Hugo que exporta estritamente código HTML estático, o Blazor WASM requer o download do runtime completo do .NET para o browser do utilizador. Estes iniciais ~4MB têm impacto na primeira interação, embora o estado seja vigorosamente armazenado em *cache* do lado do cliente para as restantes sessões.
2. **SEO em Apps Bilingues:** O meu standard de localização utiliza `localStorage` e recarrega forçadamente a página para trocar a UI, não separando efetivamente o conteúdo por rotas explícitas URL (ex. não separamos `/pt/about` de `/en/about`). Permite ter rotas de Blazor diretas e muito fáceis mas faz com que os rastreadores web indexem por excelência apeans o Idioma pré-definido (Português). Tratando-se de um blog nativo da linguagem primária interligado ao LinkedIn/GitHub, este foi um compromisso que facilmente aceitei.

## Conclusão

Construir hoje numa fração de tempo uma aplicação web totalmente profissional sem a tarefa aborrecida de teclar sintaxe pura muda substancialmente o trabalho de engenharia. Transforma o papel de "programador de código contínuo" para "arquiteto e avaliador". Se possuir um planeamento assertivo, aliando agentes de ponta e bases sólidas de conhecimento através do Microsoft Docs MCP, consigo escalar os meus próprios limites.

## Leitura Adicional
- [Microsoft Learn: ASP.NET Core Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Model Context Protocol (MCP)](https://modelcontextprotocol.io/)
- [Markdig - Markdown processor for .NET](https://github.com/xoofx/markdig)