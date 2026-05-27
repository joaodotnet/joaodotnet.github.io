---
title: "Para além do Vibe Coding: A minha experiência com Spec-Driven Development e o GitHub Spec Kit"
date: 2026-05-26
tags: [ai, software-architecture, spec-driven-development, github-spec-kit, github-copilot, developer-productivity, enterprise-software]
summary: "A programação espontânea com IA (ou 'vibe coding') é fantástica para pequenas demonstrações, mas falha à escala empresarial. Eis a minha experiência sincera a adotar o Spec-Driven Development (SDD) para trazer estrutura, previsibilidade e salvaguardas à programação assistida por IA."
slug: beyond-vibe-coding-spec-driven-development
lang: pt
coverImage: /images/profile.jpg
---

Não há muito tempo, a dificuldade no desenvolvimento de software era a escrita de código. Otimizávamos a velocidade de escrita, atalhos de teclado, macros do IDE e geradores de código repetitivo (boilerplate).

Hoje, com assistentes de programação baseados em IA como o GitHub Copilot, a escrita já não é a principal dificuldade. Um LLM pode gerar centenas de linhas de código em segundos. Em vez disso, os novos pontos de dificuldade são a **clareza, o alinhamento e as restrições**.

Quando deixamos a IA gerar código com base em prompts vagos, caímos no que a indústria chama de **"vibe coding"**. Escrevemos um prompt, inspecionamos o código, percebemos que algo está errado, escrevemos outro prompt, ajustamos e esperamos que funcione. Embora o "vibe coding" seja incrivelmente divertido para pequenos projetos pessoais e protótipos rápidos, falha espetacularmente em bases de código empresariais complexas e preexistentes (brownfield).

Recentemente, dei uma tech talk à minha equipa sobre a minha experiência real em ir além do "vibe coding". Queria partilhar por que e como adotámos o **Spec-Driven Development (SDD)** usando o **GitHub Spec Kit**, as lições que aprendemos e por que escrever especificações antes do código é a chave para libertar o verdadeiro poder da engenharia assistida por IA.

---

## O Problema do "Vibe Coding"

Quando passamos diretamente de um prompt vago para o código, a IA tem de fazer suposições. Numa configuração simples, isto pode parecer aceitável. Mas num ambiente complexo, esta falta de estrutura leva a várias dificuldades importantes:

1. **Requisitos Ambíguos**: Prompts como *"adicionar registo de auditoria"* são demasiado abertos. O que deve ser registado? Onde? Em que formato?
2. **Casos Extremos (Edge Cases) Esquecidos**: Os LLMs otimizam para o "caminho feliz" (happy path). Raramente perguntam como lidar com falhas de ligação (timeouts), impasses na base de dados (deadlocks) ou dados de entrada malformados, a menos que lhes seja dito explicitamente.
3. **Suposições Ocultas**: A IA preenche as lacunas dos requisitos com valores predefinidos que parecem plausíveis, mas que estão incorretos para o projeto.
4. **Desvio Arquitetural (Architecture Drift)**: Sem orientação, a IA tomará decisões de design prematuras e potencialmente incorretas – tais como introduzir uma nova biblioteca ou escolher um padrão de desenho (design pattern) que entra em conflito com o resto da base de código.
5. **Desvio de Documentação**: Quando o código é gerado diretamente a partir de prompts, perde-se o rasto do *porquê* de as coisas terem sido construídas de determinada forma. A intenção fica guardada no histórico do chat em vez de no repositório.
6. **Trabalho Redundante Sem Fim**: Quando os stakeholders finalmente clarificam as expectativas, acabamos por deitar fora grandes partes de código gerado.

Eis como se parece o "Ciclo de Vibe Coding" na prática:

![O Problema do Vibe Coding](/images/posts/vibe-coding-problem.svg)

---

## Entra o Spec-Driven Development (SDD)

O **Spec-Driven Development** é um processo estruturado onde a equipa define o **quê** e o **porquê** de uma funcionalidade antes de decidir **como** a vai implementar. Trata a especificação não como um PDF estático e esquecido, mas como um artefacto ativo, vivo e sob controlo de versões, que serve de contexto tanto para os programadores humanos como para os agentes de IA.

Como costumo dizer à minha equipa:
> *"Não é documentação pela documentação em si. É documentação como contexto operacional para a IA e para os humanos."*

O SDD apoia-se em três pilares fundamentais:
*   **Guiado pela Intenção**: Focamo-nos primeiro na definição dos requisitos e restrições da funcionalidade.
*   **Refinamento em Várias Etapas**: Em vez de um único prompt de geração de código direta (one-shot), refinamos a nossa compreensão através de um processo estruturado, passo a passo.
*   **Salvaguardas (Guardrails)**: Estabelecemos regras estritas e princípios de engenharia que a IA não pode violar.

### O que o SDD NÃO é
Quando os programadores ouvem a palavra "especificação", muitas vezes entram em pânico e pensam nos métodos antigos em Cascata (Waterfall). É importante clarificar:
*   **Não é Waterfall (Cascata)**: Não vais escrever um documento de 100 páginas para os próximos seis meses. Escreves especificações de forma incremental, funcionalidade a funcionalidade.
*   **Não é burocracia**: Não substitui o julgamento de engenharia; foca-o.
*   **Não é uma solução mágica**: Não garante que a IA vá escrever código perfeito, mas reduz drasticamente o espaço de procura de erros.

---

## Operacionalizar o SDD com o GitHub Spec Kit

Para implementar esta metodologia, escolhemos o [GitHub Spec Kit](https://speckit.org/), um conjunto de ferramentas de código aberto e nativo do repositório, concebido para estruturar fluxos de trabalho de desenvolvimento assistidos por IA.

O Spec Kit funciona diretamente dentro do teu repositório e integra-se com agentes de IA (como o GitHub Copilot Chat no VS Code). Impõe um **fluxo de trabalho de 6 etapas**, com cada etapa a gerar um ficheiro markdown concreto e passível de revisão num diretório dedicado `specs/`:

![Fluxo de Trabalho do GitHub Spec Kit](/images/posts/speckit-workflow.svg)

Eis como cada comando se mapeia num artefacto dentro do diretório `.specify/memory/` ou `specs/001-feature/`:

| Comando | Artefacto de Saída | Objetivo |
| :--- | :--- | :--- |
| `/speckit.constitution` | `constitution.md` | Define princípios globais do projeto, regras de segurança e padrões de código. |
| `/speckit.specify` | `spec.md` | Regista as user stories, critérios de aceitação e requisitos da funcionalidade. |
| `/speckit.clarify` | Interação / Registo de Chat | Solicita à IA que faça perguntas e identifique casos extremos esquecidos antes de o código ser escrito. |
| `/speckit.plan` | `plan.md` (mais contratos e modelos de dados) | Define a abordagem técnica, alterações de ficheiros e impacto arquitetural. |
| `/speckit.tasks` | `tasks.md` | Divide o plano numa lista de tarefas (checklist) sequencial e faseada de tarefas acionáveis. |
| `/speckit.implement` | Código Real / PR | Gera o código, ficheiros de teste e correções, limitado pelo plano e pela constituição. |

---

## A Chave para o SDD Empresarial: A Constituição

Se retiveres apenas uma coisa deste post, que seja esta: **A Constituição é o artefacto mais crítico no teu fluxo de trabalho com IA.**

Num ambiente empresarial, temos regras não negociáveis. Temos normas de conformidade (compliance), políticas de segurança, expectativas específicas de testes e stacks tecnológicas aprovadas. Sem uma Constituição, a IA não sabe nada disto. Irá sugerir com toda a confiança bibliotecas que não estás autorizado a usar, escrever testes em frameworks que não suporta ou introduzir vulnerabilidades de injeção de SQL.

> *"Sem uma constituição, a IA está a improvisar. Com uma constituição, a IA está a operar dentro do teu sistema de engenharia."*

Na Constituição da nossa equipa, codificámos regras como:
*   **Testes Unitários**: Toda a nova lógica de negócio deve ser coberta por testes unitários usando a nossa framework de testes aprovada.
*   **Segurança**: Sem segredos (secrets) codificados diretamente no código (hardcoded), e todos os endpoints de mutação de dados devem validar perfis de acesso (roles).
*   **Arquitetura**: Seguir a nossa estrutura de Clean Architecture; não contornar a camada de domínio para escrever diretamente na base de dados.
*   **Frameworks**: Não introduzir novas dependências de terceiros sem uma revisão arquitetural explícita.

Quando a IA gera um plano ou escreve código, ela verifica estas regras. Se um plano proposto sugerir a adição de um novo pacote ORM, a Constituição atua como uma salvaguarda, mantendo a IA alinhada com os padrões da nossa equipa.

---

## SDD em Projetos Brownfield

Muitas demonstrações de ferramentas de IA mostram como gerar uma aplicação simples de Tarefas do zero. Embora pareça impressionante, isso não reflete a realidade diária da maioria dos programadores. A maioria de nós trabalha em **projetos brownfield** – bases de código grandes e existentes, com dívida técnica, padrões legados e integrações complexas.

Em ambientes brownfield, o SDD é ainda mais valioso:
*   **Respeito pelas Restrições**: A IA é forçada a analisar os padrões de código existentes (por exemplo, como o tratamento de erros ou o registo de logs estão estruturados atualmente) e a escrever código que se integra perfeitamente, em vez de introduzir paradigmas em conflito.
*   **Auditabilidade**: Como cada alteração é apoiada por um `spec.md` e um `plan.md` diretamente no repositório, os revisores de pull requests podem facilmente verificar se a implementação coincide com o design acordado.
*   **Modernização Mais Segura**: Se estivermos a refatorar um módulo legado, podemos definir o comportamento antigo e as novas restrições na especificação, garantindo que a solução gerada pela IA não quebra as integrações existentes.

---

## A Minha Experiência Sincera: O Bom, o Difícil e o que Aprendi

Adotar um novo fluxo de trabalho nunca é perfeito. Eis a minha análise totalmente sincera sobre o uso do Spec-Driven Development num projeto real.

### 🟢 O Bom
*   **Qualidade Incrível do Código Gerado pela IA**: No momento em que tornámos explícitos os nossos requisitos e restrições arquiteturais, a geração de código da IA à primeira tentativa passou de "cerca de 60% correta" para "quase 90-95% correta".
*   **Menos Prompts Desperdiçados**: Deixámos de jogar o jogo de "adivinha o que eu quero" com a IA. Acabaram-se os ciclos infinitos de ajuste de detalhes no prompt porque a IA se esqueceu de uma regra.
*   **Entendimento Partilhado**: Discutir o design de uma funcionalidade usando o `spec.md` e o `plan.md` num Pull Request *antes* de escrever o código é muito mais rápido e barato do que rever um diff de 2000 linhas mais tarde.
*   **Decomposição**: Ter a IA a gerar um `tasks.md` estruturado ajudou-nos a identificar quais as etapas que podiam ser executadas em paralelo e a abordar a implementação de forma sistemática sem perder o contexto.

### 🔴 O Difícil
*   **Sobrecarga Inicial (Overhead)**: Escrever uma especificação, executar ciclos de clarificação e esperar que a IA elabore um plano leva o seu tempo. Para correções de bugs simples de 10 minutos, este processo parece demasiado pesado.
*   **A Tentação do Programador**: Como programadores, o nosso instinto é saltar diretamente para o editor e começar a escrever código. Requer disciplina de equipa para abrandar, escrever a especificação e rever o plano primeiro.
*   **Verificação do Plano**: Os planos técnicos gerados pela IA ainda requerem uma revisão humana crítica. Se aceitares um plano cegamente, a etapa de implementação irá apenas codificar os erros da IA.
*   **Manutenção da Especificação**: Se um requisito mudar a meio do desenvolvimento, é necessário atualizar a especificação e o plano. Caso contrário, eles divergem e a IA perde a sua fonte de verdade.

### 💡 O que Aprendi
1. **Dimensionar adequadamente o processo**: Não uses o SDD para tudo. É mais valioso quando a lógica de negócio é complexa, os requisitos são ambíguos ou o impacto arquitetural é elevado. Para pequenos ajustes, simplifica.
2. **A Constituição é um documento vivo**: Atualizamos a Constituição do nosso projeto à medida que aprendemos novos padrões ou descobrimos antipadrões. Torna-se o cérebro partilhado da equipa.
3. **A IA é um assistente, não um agente autónomo**: O SDD foca-se no *desenvolvimento guiado*. Tu és o arquiteto e o diretor; a IA é um construtor altamente eficiente. Tens de fornecer o plano (blueprint).

---

## O Panorama Geral: Para Onde Caminha a Indústria?

O GitHub Spec Kit não é o único player neste espaço. Toda a indústria de engenharia de software está a convergir para abordagens guiadas por especificações:

*   **Kiro (AWS)**: Uma experiência de IDE baseada em agentes (agêntica, baseada num fork do VS Code) que incorpora especificações, regras de condução globais e ganchos (hooks) de automatização orientados a eventos diretamente no editor.
*   **BMAD**: O *Breakthrough Method for Agile AI-Driven Development* é uma framework de código aberto que organiza agentes de IA especializados em papéis ágeis virtuais (Product Managers, Arquitetos, Scrum Masters e QA), imitando uma equipa de produto humana para gerar especificações estruturadas e ficheiros de histórias antes de escrever o código.
*   **OpenSpec**: Um padrão aberto e neutro emergente para definir especificações de software num formato legível por máquina que qualquer agente de IA possa consumir.

Esta convergência prova que o SDD não é apenas uma tendência passageira. É a evolução lógica da engenharia de software na era da IA generativa.

---

## Um Caminho de Adoção para a Tua Equipa

Se queres introduzir o Spec-Driven Development na tua equipa, não precisas de adotar tudo de uma vez no primeiro dia. Podes seguir um modelo de maturidade gradual:

![Modelo de Maturidade de Adoção do SDD](/images/posts/maturity-model.svg)

Começa no **Nível 2** ou **Nível 3**. Introduz um ficheiro básico de diretrizes de projeto (uma Constituição) e escreve uma especificação markdown simples para a tua próxima grande funcionalidade. Vê como a IA responde, mede a qualidade do código gerado e itera a partir daí.

---

## Conclusão

Estamos a afastar-nos da era do "vibe coding". À medida que os modelos de IA se tornam mais rápidos e baratos, os programadores que se vão destacar não serão aqueles que conseguem gerar código mais rapidamente, mas sim os que conseguem definir requisitos, estruturar a arquitetura e impor salvaguardas de engenharia de forma mais eficaz.

O Spec-Driven Development dá-nos o vocabulário, o fluxo de trabalho e a disciplina para fazer exatamente isso. Ao escrever primeiro as especificações, guiamos a IA para construir o software correto, da forma correta, à primeira.

---

### Recursos Adicionais
- [Site Oficial do GitHub Spec Kit](https://speckit.org/)
- [Repositório do GitHub Spec Kit](https://github.com/github/spec-kit)
- [Microsoft Learn: Spec-Driven Development para Programadores Empresariais](https://learn.microsoft.com/en-us/training/modules/spec-driven-development-github-spec-kit-enterprise-developers/)
- [GitHub Blog: Spec-Driven Development com IA](https://github.blog/ai-and-ml/generative-ai/spec-driven-development-with-ai-get-started-with-a-new-open-source-toolkit/)
- [Documentação do Kiro (AWS)](https://kiro.dev/docs/)

---

*Este post foi inspirado na minha tech talk recente na GlinttGlobal.*
