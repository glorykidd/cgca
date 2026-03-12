# Cedar Grove Christian Academy Website

## Summary

The CGCA website is a server-rendered Blazor application built on .NET 10.0 with an interactive WebAssembly island for the AI chatbot. It serves as the public-facing site for Cedar Grove Christian Academy, providing SEO-optimized, marketing-focused content about the school's mission, programs, admissions, and values to prospective and current families. The site includes pages for school information, programs, admissions with tuition details, parent resources, an embedded event calendar, a contact form, and a privacy policy. Third-party services — Google Calendar, Google Forms, and Praxis School — are integrated via embedded content and external links. An AI chatbot ("Gracie") powered by a Cloudflare Worker backend assists visitors with common questions.

## Technology Overview

| Layer | Technology |
|---|---|
| Framework | .NET 10.0 / ASP.NET Core Blazor (SSR + Interactive WASM) |
| Language | C# (nullable reference types, implicit usings) |
| UI Components | Blazor Bootstrap 3.5.0, Bootstrap 5.3.7 native modals |
| CSS Framework | Bootstrap 5.3.7 (CDN) |
| Icons | Bootstrap Icons 1.13.1 (CDN) |
| Analytics | Google Analytics 4 with custom event tracking |
| AI Chatbot | Cloudflare Worker backend (Claude Haiku), Blazor WASM interactive widget |
| Testing | xUnit, bUnit 2.5.3, FluentAssertions 8.8.0 |
| CI/CD | GitHub Actions — self-hosted Windows runner, IIS + Cloudflare Workers deployment |
| License | MIT |

Pages are statically rendered on the server (SSR) for fast load times and full SEO crawlability. The AI chatbot runs as an Interactive WebAssembly island — the only component requiring client-side interactivity. There is no database; all content is static HTML within Razor components, supplemented by embedded third-party services.

## Architecture Overview

The application uses a three-project architecture under `cgca.new/`:

- **`cgca.web`** — ASP.NET Core server that statically renders all pages and serves the WASM runtime for interactive components
- **`cgca.web.client`** — Blazor WebAssembly project containing the interactive chatbot widget and its supporting services/models
- **`cgca-chatbot-worker`** — Cloudflare Worker that proxies chat requests to the Anthropic Claude API and handles lead capture

```
cgca.new/
├── cgca.sln                          # Solution file
├── cgca.web/                         # Server project (SSR host)
│   ├── Program.cs                    # ASP.NET Core host — middleware, service registration
│   ├── App.razor                     # Root HTML document — meta tags, structured data, GA4, CDN refs
│   ├── Routes.razor                  # Blazor router with NotFound fallback
│   ├── _Imports.razor                # Global using statements
│   ├── Layout/
│   │   ├── MainLayout.razor          # Shared page shell (nav, footer, social links, chatbot)
│   │   └── NavMenu.razor             # Collapsible navigation bar
│   ├── Pages/                        # Routable page components (static SSR)
│   │   ├── Home.razor                # Landing page — hero, quick facts, benefits, ABCs, CTAs
│   │   ├── About.razor               # Mission, vision, philosophy, statement of faith, conduct
│   │   ├── Programs.razor            # K3 and K4/K5 program details and schedules
│   │   ├── Admissions.razor          # Enrollment steps, tuition/fees, program cards
│   │   ├── Parents.razor             # Parent resources, tuition details, portal link
│   │   ├── Contact.razor             # Contact info, Google Maps, newsletter, Google Form
│   │   ├── Calendar.razor            # Embedded Google Calendar
│   │   └── Privacy.razor             # Privacy policy
│   └── wwwroot/                      # Static assets
│       ├── css/app.css               # Global stylesheet
│       ├── images/                   # Optimized site images
│       ├── robots.txt                # Search engine crawl directives
│       └── sitemap.xml               # XML sitemap for SEO
├── cgca.web.client/                  # WASM client project (interactive island)
│   ├── Program.cs                    # WASM host — HttpClient, ChatService registration
│   ├── _Imports.razor                # Client-side usings
│   ├── Components/
│   │   ├── ChatWidget.razor          # AI chatbot widget (Gracie)
│   │   └── ChatWidget.razor.css      # Chatbot scoped styles
│   ├── Models/                       # Chat data models
│   │   ├── ChatMessage.cs
│   │   ├── ChatRequest.cs
│   │   ├── ChatResponse.cs
│   │   └── LeadRequest.cs
│   ├── Services/                     # Chat API services
│   │   ├── IChatService.cs
│   │   └── ChatService.cs
│   └── wwwroot/
│       └── appsettings.json          # ChatApiBaseUrl configuration
├── cgca-chatbot-worker/              # Cloudflare Worker (chatbot backend)
│   ├── src/
│   │   ├── index.js                  # Main Worker — chat + leads route handlers, CORS
│   │   ├── systemPrompt.js           # CGCA system prompt for Claude
│   │   ├── rateLimit.js              # IP cooldown + session message cap via KV
│   │   └── cache.js                  # Common question response caching via KV
│   ├── wrangler.toml                 # Cloudflare Worker config, KV namespace bindings
│   ├── package.json
│   ├── package-lock.json
│   └── README.md                     # Worker-specific setup instructions
└── cgca.web.Tests/                   # Unit test project
    ├── StubChatService.cs            # Mock chat service for testing
    ├── Components/
    │   ├── AppTests.cs               # Routes component and router tests
    │   ├── NavMenuTests.cs           # Navigation links, toggle, rendering
    │   └── MainLayoutTests.cs        # Layout structure and content rendering
    ├── Pages/
    │   └── PageRenderingTests.cs     # All pages render without exceptions
    └── Integration/
        └── RoutingTests.cs           # Route resolution for all 8 routes
```

### Rendering Model

- **Static SSR** — All pages are rendered on the server and delivered as complete HTML. This provides instant page loads and allows search engines to crawl all content without executing JavaScript.
- **Interactive WASM Island** — The chatbot widget (`ChatWidget.razor`) is the sole interactive component, rendered with `InteractiveWebAssemblyRenderMode(prerender: false)` to run client-side only.
- **Native Bootstrap modals** — Modals on About and Parents pages use Bootstrap 5 `data-bs-toggle`/`data-bs-target` attributes rather than Blazor interactive components, so they work without a client-side runtime.
- **Scoped CSS** — Both `cgca.web.styles.css` and `cgca.web.client.styles.css` must be loaded in `App.razor` for component styles to apply correctly.

### SEO Features

- Per-page `<title>`, `<meta description>`, Open Graph, and canonical tags via `<HeadContent>`
- JSON-LD structured data (`EducationalOrganization` schema) in `App.razor`
- `robots.txt` and `sitemap.xml` covering all 8 routes
- Server-rendered HTML for full search engine crawlability
- Skip-to-content link for accessibility

### Analytics & Conversion Tracking

- Google Analytics 4 (GA4) with tag `G-0ZQ2NMY9F1`
- Custom `cgcaTrack()` JavaScript helper for event tracking
- Automatic click tracking for: enrollment, donation, newsletter signup, contact, and admissions CTAs
- Chatbot events: `chatbot_open`, `lead_captured`

## Pages

| Route | Page | Description |
|---|---|---|
| `/` | Home | Hero banner, quick facts, "Why Choose CGCA" benefits, ABCs, enrollment CTA |
| `/about` | About Us | Mission statement, vision, philosophy, statement of faith, code of conduct (modals) |
| `/programs` | Programs | K3 and K4/K5 program details, schedules, curriculum overview |
| `/admissions` | Admissions | 3-step enrollment process, tuition & fees tables, program cards |
| `/parents` | Parents | Parent resources, tuition modal, school calendar link, parent portal |
| `/contact` | Contact | Address, phone, email, hours, Google Maps embed, newsletter, Google Form |
| `/calendar` | Calendar | Embedded Google Calendar |
| `/privacy` | Privacy | Privacy policy |

## AI Chatbot (Gracie)

The chatbot is an interactive Blazor WASM component that communicates with a Cloudflare Worker backend. It provides:

- Answers to common questions about admissions, tuition, school hours, and programs
- Lead capture form (name + email) after 2 user messages, forwarded to Google Sheets via Apps Script webhook
- Session-based conversation with a 15-message cap per session
- IP rate limiting (1 request per 2 seconds)
- Response caching for common questions (24-hour TTL)
- GA4 event tracking for opens and lead captures
- Graceful error handling with CORS headers always returned

### Chatbot Architecture

```
Browser (WASM)          Cloudflare Worker              Anthropic API
ChatWidget.razor  →  POST /api/chat  →  Claude Haiku (claude-haiku-4-5-20251001)
                     POST /api/leads →  Google Sheets webhook (fallback: KV storage)
```

### Configuration

- **Worker URL**: Set `ChatApiBaseUrl` in `cgca.web.client/wwwroot/appsettings.json`
- **Worker secrets** (stored via `wrangler secret put`):
  - `ANTHROPIC_API_KEY` — Anthropic API key for Claude
  - `GOOGLE_SHEETS_WEBHOOK_URL` — Apps Script webhook for lead capture
- **KV namespaces**: `CHAT_CACHE` (response cache + lead fallback), `RATE_LIMIT` (IP + session counters)
- **Cost controls**: Claude Haiku model, 350 max tokens, 6-message history trim, $30/month spend cap in Anthropic console

### CORS

The Worker allows requests from:
- `https://cedargrovechristianacademy.org`
- `https://www.cedargrovechristianacademy.org`
- `http://localhost:5297` (dev)
- `https://localhost:7183` (dev)

## Unit Tests

The project includes **31 tests** across three categories built with **bUnit**, **xUnit**, and **FluentAssertions**.

### Test Coverage

#### 1. Component Tests
- **NavMenuTests**: Toggle functionality, navigation links, logo rendering
- **AppTests**: Routes component rendering, router verification, NotFound configuration
- **MainLayoutTests**: Layout rendering, NavMenu inclusion, body content, container verification

#### 2. Page Rendering Tests
- Verifies all 8 pages (Home, About, Programs, Admissions, Parents, Contact, Calendar, Privacy) render without exceptions

#### 3. Routing Tests
- Validates all 8 routes resolve correctly
- Confirms router configuration and route-to-component mapping

### Running Tests

```bash
# Run all tests
dotnet test cgca.new/cgca.sln

# Run tests with detailed output
dotnet test cgca.new/cgca.sln --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~NavMenuTests"
```

| Framework | Purpose |
|---|---|
| **xUnit** | Core test framework |
| **bUnit 2.5.3** | Blazor component rendering and interaction testing |
| **FluentAssertions 8.8.0** | Readable, expressive assertion syntax |

## Commands

All commands run from `cgca.new/`:

```bash
# Build the solution
dotnet build cgca.sln

# Run tests
dotnet test cgca.sln

# Start the dev server
cd cgca.web
dotnet run

# Deploy chatbot worker manually
cd cgca-chatbot-worker
npm install
npx wrangler deploy
```

The dev server starts on **http://localhost:5297** and **https://localhost:7183**.

## Branching

- `main` — production branch; pushes trigger the deployment pipeline
- `develop` — primary development branch; use as the base for all pull requests

Create feature branches off `develop` and open PRs back into `develop`.

## CI/CD & Deployment

Two GitHub Actions workflows (`.github/workflows/`) automate builds and deployments on a self-hosted Windows runner:

### Develop Branch (`cgca-develop.yml`)

Triggers on push to `develop` with changes under `cgca.new/`:

1. Checkout code (full history)
2. Run unit tests (`dotnet test` in Release)
3. Build full solution (`dotnet build` in Release)
4. Email build report

### Production (`cgca.yml`)

Triggers on push to `main` with changes under `cgca.new/`. Two jobs run sequentially:

**Job 1: `deploy-website`**
1. Checkout code (full history for commit reporting)
2. Run unit tests — deployment aborts if tests fail
3. Generate timestamped build number
4. Create backup of current production site (compressed archive)
5. Stop IIS app pool (`CedarGroveChristianAcademy`)
6. Publish to production (`dotnet publish -c Release`)
7. Start IIS app pool
8. Email deployment report

**Job 2: `deploy-chatbot`** (runs only after website succeeds)
1. Checkout code
2. Install worker dependencies (`npm ci`)
3. Deploy Cloudflare Worker (`npx wrangler deploy`)

### Required Secrets

| Secret | Location | Purpose |
|---|---|---|
| `CLOUDFLARE_API_TOKEN` | GitHub Actions | Authenticates `wrangler deploy` for the chatbot worker |
| `ANTHROPIC_API_KEY` | Cloudflare Worker | API key for Claude Haiku chat completions |
| `GOOGLE_SHEETS_WEBHOOK_URL` | Cloudflare Worker | Apps Script webhook for lead capture |
