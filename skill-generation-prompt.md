# Skill Generation Prompt

Use this prompt with any LLM (GPT-4, Claude, Gemini) to generate a comprehensive skills database for a recruitment platform.

---

## Prompt

```
You are generating a comprehensive skills reference database for a tech recruitment platform called JobIntel. The platform covers 9 job role families:

1. **Frontend** (Frontend = 1) — UI frameworks, CSS, browsers, design tools
2. **Backend** (Backend = 2) — Server frameworks, APIs, databases, caching
3. **FullStack** (FullStack = 3) — Overlap of Frontend + Backend + DevOps
4. **Mobile** (Mobile = 4) — iOS, Android, cross-platform, mobile tools
5. **Data** (Data = 5) — Data science, ML/AI, analytics, BI, data engineering
6. **DevOps** (DevOps = 6) — CI/CD, cloud, containers, IaC, monitoring, SRE
7. **QA** (QA = 7) — Testing frameworks, automation, test management, performance testing
8. **Design** (Design = 8) — UI/UX tools, graphic design, prototyping, motion design
9. **Other** (Other = 9) — Soft skills, project management, business tools, CRM, collaboration

## Output Format

Return a JSON array of objects. Each object has exactly 2 fields:

```json
[
  { "name": "Skill Name" },
  { "name": "Another Skill" }
]
```

## Rules

1. **No duplicates** — each skill appears exactly once
2. **Canonical names** — use the industry-standard name (e.g. "React" not "ReactJS", "ASP.NET Core" not "Asp.net core", "Node.js" not "nodejs")
3. **No versions** — exclude version numbers (e.g. "React 18" → "React")
4. **Single entries** — "HTML/CSS" is one entry, not "HTML" and "CSS" separately
5. **Be comprehensive** — aim for 500-700 total skills across ALL categories
6. **Cover each domain** — generate skills for ALL 9 role families, not just engineering
7. **Include missing common skills** — GitHub, Bootstrap, SASS/SCSS, Redux, jQuery, RabbitMQ, Visual Studio, VS Code, Puppet, Chef, Vagrant, WordPress, Shopify, Stripe, Twilio, Slack, Microsoft Teams, Zoom, Figma, Adobe XD, and similar widely-used tools
8. **Include soft skills** — communication, leadership, problem-solving, etc.
9. **Include methodologies** — Agile, Scrum, Kanban, Lean, SAFe, etc.
10. **Include certifications if relevant** — AWS Certified, PMP, CISSP, etc.

## Categories (use as comments in output)

Group skills under these categories with `//` comments:

```
// ─────────────────────────────────────────────
// SOFTWARE ENGINEERING — Programming Languages
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SOFTWARE ENGINEERING — Frontend
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SOFTWARE ENGINEERING — Backend and Frameworks
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SOFTWARE ENGINEERING — Databases and Storage
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SOFTWARE ENGINEERING — Architecture and APIs
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SOFTWARE ENGINEERING — Testing and Quality
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SOFTWARE ENGINEERING — Mobile
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SOFTWARE ENGINEERING — Tools and General
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// AI and DATA — Core
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// AI and DATA — Frameworks and Libraries
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// AI and DATA — Data Engineering and BI
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// CLOUD & DEVOPS — Platforms
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// CLOUD & DEVOPS — Containers & Orchestration
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// CLOUD & DEVOPS — IaC & Automation
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// CLOUD & DEVOPS — CI/CD & Monitoring
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// CYBERSECURITY
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// UI/UX & DESIGN — Tools
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// UI/UX & DESIGN — Disciplines
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// MARKETING
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// PRODUCT MANAGEMENT
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// BUSINESS & STRATEGY
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SALES
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// CONTENT CREATION
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// OPERATIONS
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// EMERGING TECH
// ─────────────────────────────────────────────

// ─────────────────────────────────────────────
// SOFT SKILLS & CROSS-DOMAIN
// ─────────────────────────────────────────────
```

## Example (first 10 entries)

```json
[
  { "name": "C#" },
  { "name": "JavaScript" },
  { "name": "TypeScript" },
  { "name": "Python" },
  { "name": "Java" },
  { "name": "C++" },
  { "name": "PHP" },
  { "name": "Ruby" },
  { "name": "Go" },
  { "name": "Swift" }
]
```

## Critical: What to ADD (commonly missing)

These skills appear frequently on real CVs and job postings but are often missed. ENSURE they are included:

**Frontend:** Bootstrap, SASS/SCSS, CSS3, Material UI, Chakra UI, Redux, Zustand, MobX, Alpine.js, AlpineJS, jQuery, Web Components, Stencil.js, Ionic, Nuxt.js (if missing), Remix, Astro, Solid.js, Qwik

**Backend:** RabbitMQ, MassTransit, Hangfire, MediatR, Carter, Minimal APIs, gRPC (if missing), SignalR (if missing), GraphQL (if missing), OData, Blazor Server, Blazor WebAssembly

**DevOps:** GitHub Actions (if missing), GitLab CI (if missing), Jenkins (if missing), ArgoCD (if missing), Vault (if missing), Packer, Vagrant, Pulumi, Crossplane, Flux, Sealed Secrets, External Secrets

**Data:** Airflow (if missing), dbt (if missing), Prefect, Dagster, Mage, Great Expectations, Monte Carlo, Atlan, DataHub, Amundsen

**Tools:** Visual Studio, VS Code, IntelliJ IDEA, Android Studio, Xcode, Postman (if missing), Insomnia, DBeaver, DataGrip, pgAdmin

**Cloud:** Cloudflare, Fastly, Akamai, Vercel, Netlify, Fly.io, Railway, Render, Heroku

**CRM/Sales:** Salesforce (if missing), HubSpot (if missing), Pipedrive, Zoho CRM, Freshsales, Outreach, SalesLoft, Gong, Chorus

**Collaboration:** Slack (if missing), Microsoft Teams (if missing), Zoom (if missing), Loom, Miro, FigJam, Notion, Coda, Airtable, ClickUp, Monday.com, Asana, Basecamp, Trello (if missing), Jira (if missing), Confluence (if missing)

**Design:** Framer, Webflow, Sketch (if missing), InVision (if missing), Principle, ProtoPie, Maze, Hotjar, FullStory, Mixpanel, Amplitude

**Soft Skills:** Remote Work, Cross-cultural Communication, Adaptability, Creativity, Attention to Detail, Self-motivation, Work Ethic, Interpersonal Skills, Presentation Skills, Negotiation (if missing)

**Methodologies:** Scrum (if missing), Kanban, SAFe, Lean, XP, Crystal, Feature-Driven Development, Pair Programming, Mob Programming, Trunk-Based Development

**Certifications:** AWS Certified Solutions Architect, AWS Certified Developer, Azure Fundamentals, Azure Developer Associate, GCP Professional, Kubernetes Administrator (CKA), Kubernetes Application Developer (CKAD), Certified Scrum Master, PMP, CompTIA Security+, CISSP, CEH, Google Analytics Certified, HubSpot Certified, Salesforce Certified

---

Generate the complete JSON array now. Be comprehensive. Target 600+ skills.
```

---

## How to Use

1. Copy the prompt above
2. Paste into ChatGPT, Claude, or Gemini
3. Copy the JSON output
4. Convert to C# using the converter script below (or do it manually)
5. Add to `SkillSeed.cs` with the next available Id

## Quick C# Converter

If the LLM outputs just `["Skill1", "Skill2", ...]`, convert with:

```bash
# PowerShell one-liner (adjust startId)
$startId = 351
$i = $startId
Get-Content skills.json | ConvertFrom-Json | ForEach-Object {
    "                new() { Id = $i, Name = `"$_`", CreatedAt = SeedCreatedAt },"
    $i++
}
```

Or if it outputs `[{"name": "Skill1"}, ...]`:

```bash
$startId = 351
$i = $startId
Get-Content skills.json | ConvertFrom-Json | ForEach-Object {
    "                new() { Id = $i, Name = `"$($_.name)`", CreatedAt = SeedCreatedAt },"
    $i++
}
```
