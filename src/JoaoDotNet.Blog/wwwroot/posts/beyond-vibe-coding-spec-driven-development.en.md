---
title: "Beyond Vibe Coding: My Experience with Spec-Driven Development and GitHub Spec Kit"
date: 2026-05-26
tags: [ai, software-architecture, spec-driven-development, github-spec-kit, github-copilot, developer-productivity, enterprise-software]
summary: "Freestyle AI coding (or vibe coding) is great for small demos, but fails at enterprise scale. Here is my honest experience adopting Spec-Driven Development (SDD) to bring structure, predictability, and guardrails to AI-assisted coding."
slug: beyond-vibe-coding-spec-driven-development
lang: en
coverImage: /images/profile.jpg
---

Not long ago, the bottleneck in software development was typing. We optimized for typing speed, keyboard shortcuts, IDE macros, and boilerplate generators. 

Today, with AI coding assistants like GitHub Copilot, typing is no longer the bottleneck. An LLM can generate hundreds of lines of code in seconds. Instead, the new bottlenecks are **clarity, alignment, and constraints**.

When we let AI generate code based on vague prompts, we fall into what the industry calls **"vibe coding"**. We prompt, inspect the code, notice something is wrong, prompt again, tweak, and hope it works. While vibe coding is incredibly fun for small side projects and quick prototypes, it breaks down spectacularly in professional, brownfield enterprise codebases.

Recently, I gave a tech talk to my team about my real-world experience moving beyond vibe coding. I wanted to share why and how we adopted **Spec-Driven Development (SDD)** using **GitHub Spec Kit**, the lessons we learned, and why writing specifications before code is the key to unlocking the true power of AI-assisted engineering.

---

## The Problem with "Vibe Coding"

When you go straight from a vague prompt to code, the AI has to make assumptions. In a simple setup, this might seem fine. But in a complex environment, this lack of structure leads to several major pain points:

1. **Ambiguous Requirements**: Prompts like *"add audit logging"* are too open-ended. What should be logged? Where? In what format?
2. **Missing Edge Cases**: LLMs optimize for the "happy path." They rarely ask you how to handle network timeouts, database deadlocks, or malformed inputs unless you explicitly tell them to.
3. **Hidden Assumptions**: The AI fills requirement gaps with plausible-sounding but project-incorrect defaults.
4. **Architecture Drift**: Without guidance, the AI will make premature and potentially incorrect design decisions - such as introducing a new library or choosing a design pattern that conflicts with the rest of your codebase.
5. **Documentation Drift**: When code is generated directly from prompts, you lose the trace of *why* things were built a certain way. The intent remains locked in your chat history instead of your repository.
6. **Endless Rework**: When stakeholders finally clarify the expectations, you end up throwing away large chunks of generated code.

Here is what the "Vibe Coding Cycle" looks like in practice:

![The Problem with Vibe Coding](/images/posts/vibe-coding-problem.svg)

---

## Enter Spec-Driven Development (SDD)

**Spec-Driven Development** is a structured process where the team defines the **what** and **why** of a feature before deciding **how** to implement it. It treats the specification not as a static, forgotten PDF, but as an active, living, version-controlled artifact that serves as context for both human developers and AI agents.

As I like to say to my team:
> *"It’s not documentation for documentation’s sake. It’s documentation as operational context for AI and humans."*

SDD stands on three core pillars:
*   **Intent-Driven**: We focus on defining the feature's requirements and constraints first.
*   **Multi-Step Refinement**: Instead of a single one-shot code generation prompt, we refine our understanding through a structured, step-by-step process.
*   **Guardrails**: We establish strict rules and engineering principles that the AI is not allowed to violate.

### What SDD is NOT
When developers hear the word "spec," they often panic and think of old-school Waterfall methods. It is important to clarify:
*   **It is not Waterfall**: You don't write a 100-page document for the next six months. You write specs incrementally, feature by feature.
*   **It is not bureaucracy**: It doesn't replace engineering judgment; it focuses it.
*   **It is not a silver bullet**: It doesn't guarantee the AI will write perfect code, but it drastically reduces the search space for errors.

---

## Operationalizing SDD with GitHub Spec Kit

To implement this methodology, we chose [GitHub Spec Kit](https://speckit.org/), an open-source, repo-native toolkit designed to structure AI-assisted development workflows.

Spec Kit works directly inside your repository and integrates with AI agents (such as GitHub Copilot Chat in VS Code). It enforces a **6-step workflow**, with each step generating a concrete, reviewable markdown file in a dedicated `specs/` directory:

![GitHub Spec Kit Workflow](/images/posts/speckit-workflow.svg)

Here is how each command maps to an artifact inside the `.specify/memory/` or `specs/001-feature/` directory:

| Command | Output Artifact | Purpose |
| :--- | :--- | :--- |
| `/speckit.constitution` | `constitution.md` | Defines project-wide principles, security rules, and coding standards. |
| `/speckit.specify` | `spec.md` | Captures user stories, acceptance criteria, and feature requirements. |
| `/speckit.clarify` | Chat Interaction / Log | Prompts the AI to ask questions and identify missing edge cases before code is written. |
| `/speckit.plan` | `plan.md` (plus contracts & data models) | Defines the technical approach, file changes, and architectural impact. |
| `/speckit.tasks` | `tasks.md` | Breaks down the plan into a phased, sequential checklist of actionable tasks. |
| `/speckit.implement` | Real Code / PR | Generates the code, test files, and fixes, constrained by the plan and constitution. |

---

## The Key to Enterprise SDD: The Constitution

If you take only one thing from this post, let it be this: **The Constitution is the most critical artifact in your AI workflow.**

In an enterprise environment, we have non-negotiable rules. We have compliance standards, security policies, specific testing expectations, and approved tech stacks. Without a Constitution, the AI doesn't know any of this. It will confidently suggest libraries you aren't allowed to use, write tests in frameworks you don't support, or introduce SQL injection vulnerabilities.

> *"Without a constitution, the AI is improvising. With a constitution, the AI is operating inside your engineering system."*

In our team's Constitution, we encoded rules like:
*   **Unit Testing**: All new business logic must be covered by unit tests using our approved testing framework.
*   **Security**: No hardcoded secrets, and all data mutation endpoints must validate roles.
*   **Architecture**: Follow our Clean Architecture layout; do not bypass the domain layer to write directly to the database.
*   **Frameworks**: Do not introduce new third-party dependencies without explicit architectural review.

When the AI generates a plan or writes code, it checks these rules. If a proposed plan suggests adding a new ORM package, the Constitution acts as a guardrail, keeping the AI aligned with our team's standards.

---

## SDD in Brownfield Projects

Many AI tool demos show you how to generate a simple Todo application from scratch. While that looks impressive, it doesn't reflect the daily reality of most developers. Most of us work on **brownfield projects** - large, existing codebases with technical debt, legacy patterns, and complex integrations.

In brownfield environments, SDD is even more valuable:
*   **Respecting Constraints**: The AI is forced to analyze the existing code patterns (e.g. how error handling or logging is currently structured) and write code that blends in seamlessly, rather than introducing conflicting paradigms.
*   **Auditability**: Because every change is backed by a `spec.md` and a `plan.md` right in the repo, pull request reviewers can easily verify if the implementation matches the agreed-upon design.
*   **Safer Modernization**: If we are refactoring a legacy module, we can define the old behavior and the new constraints in the spec, ensuring the AI-generated solution doesn't break existing integrations.

---

## My Honest Experience: The Good, the Difficult, and the Learned

Adopting a new workflow is never perfect. Here is my completely honest review of using Spec-Driven Development in a real-world project.

### 🟢 The Good
*   **Incredible AI Output Quality**: The moment we made our requirements and architectural constraints explicit, the AI's first-pass code generation went from "roughly 60% correct" to "almost 90-95% correct."
*   **Less Wasted Prompting**: We stopped playing the "guess what I want" game with the AI. No more endless cycles of tweaking prompt details because the AI forgot a rule.
*   **Shared Understanding**: Discussing a feature design using the `spec.md` and `plan.md` in a Pull Request *before* writing code is much faster and cheaper than reviewing a 2,000-line diff later.
*   **Decomposition**: Having the AI generate a structured `tasks.md` helped us identify which steps could be run in parallel and tackle the implementation systematically without losing context.

### 🔴 The Difficult
*   **Initial Overhead**: Writing a spec, running clarification loops, and waiting for the AI to draft a plan takes time. For simple 10-minute bug fixes, this process feels far too heavy.
*   **Developer Temptation**: As developers, our instinct is to jump straight into the editor and start writing code. It requires team discipline to slow down, write the spec, and review the plan first.
*   **Plan Verification**: The AI's generated technical plans still require critical human review. If you blindly accept a plan, the implementation step will just codify the AI's mistakes.
*   **Spec Maintenance**: If a requirement changes midway through development, you must update the spec and the plan. Otherwise, they drift, and the AI loses its source of truth.

### 💡 What I Learned
1. **Right-size the process**: Don't use SDD for everything. It is most valuable when the business logic is complex, the requirements are ambiguous, or the architectural impact is high. For minor tweaks, keep it light.
2. **The Constitution is a living document**: We update our project's Constitution as we learn new patterns or discover anti-patterns. It becomes the team's shared brain.
3. **AI is an assistant, not an autonomous agent**: SDD is about *guided development*. You are the architect and the director; the AI is a highly efficient builder. You must provide the blueprint.

---

## The Broader Landscape: Where is the Industry Heading?

GitHub Spec Kit isn't the only player in this space. The entire software engineering industry is converging on spec-driven approaches:

*   **Kiro (AWS)**: An agentic IDE experience (a fork of VS Code) that embeds specs, global steering rules, and event-driven automation hooks directly into the editor.
*   **BMAD**: The *Breakthrough Method for Agile AI-Driven Development* is an open-source framework that organizes specialized AI agents into virtual agile roles (Product Managers, Architects, Scrum Masters, and QA), mimicking a human product team to generate structured specs and story files before writing code.
*   **OpenSpec**: An emerging open, vendor-neutral standard for defining software specifications in an machine-readable format that any AI agent can consume.

This convergence proves that SDD is not just a passing trend. It is the logical evolution of software engineering in the age of generative AI.

---

## An Adoption Path for Your Team

If you want to introduce Spec-Driven Development to your team, you don't need to go all-in on day one. You can follow a gradual maturity model:

![SDD Adoption Maturity Model](/images/posts/maturity-model.svg)

Start at **Level 2** or **Level 3**. Introduce a basic project guidelines file (a Constitution) and write a simple markdown spec for your next major feature. See how the AI responds, measure the quality of the generated code, and iterate from there.

---

## Conclusion

We are moving away from the era of "vibe coding." As AI models become faster and cheaper, the developers who stand out will not be those who can generate code the quickest, but those who can define requirements, structure architecture, and enforce engineering guardrails the most effectively.

Spec-Driven Development gives us the vocabulary, the workflow, and the discipline to do exactly that. By writing specifications first, we guide the AI to build the right software, the right way, the first time.

---

### Additional Resources
- [GitHub Spec Kit Official Site](https://speckit.org/)
- [GitHub Spec Kit Repository](https://github.com/github/spec-kit)
- [Microsoft Learn: Spec-Driven Development for Enterprise Developers](https://learn.microsoft.com/en-us/training/modules/spec-driven-development-github-spec-kit-enterprise-developers/)
- [GitHub Blog: Spec-Driven Development with AI](https://github.blog/ai-and-ml/generative-ai/spec-driven-development-with-ai-get-started-with-a-new-open-source-toolkit/)
- [Kiro (AWS) Documentation](https://kiro.dev/docs/)

---

*This post was inspired by my recent tech talk at GlinttGlobal.*
