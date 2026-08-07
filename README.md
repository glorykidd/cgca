# Cedar Grove Christian Academy Website

## Summary

The CGCA website is a hybrid Blazor application built on .NET 10.0. It serves as the public-facing site for Cedar Grove Christian Academy, providing SEO-optimized content about the school's mission, programs, admissions, and values to prospective and current families. The site includes pages for school information, programs, admissions with tuition details, parent resources, an embedded event calendar, a contact/sponsorship form, and a privacy policy. Third-party services — Google Calendar and Praxis School — are integrated via embedded content and external links. An AI chatbot ("Gracie") powered by a Cloudflare Worker backend assists visitors with common questions.

A server-side admin section allows staff to manage contact and sponsorship inquiries, including read/acknowledged tracking, lifecycle status tracking for sponsorships, threaded notes, and email forwarding/reply capabilities.

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

Public pages are statically rendered on the server (SSR) for fast load times and full SEO crawlability. The AI chatbot runs as an Interactive WebAssembly island. Admin pages use Interactive Server render mode backed by a SQLite database via EF Core.

## Architecture Overview

The application uses a three-project architecture under `cgca.new/`:

- **`cgca.web`** — ASP.NET Core server that renders all public pages (SSR) and admin pages (Interactive Server), hosts the EF Core/SQLite data layer, and serves the WASM runtime for the chatbot island
- **`cgca.web.client`** — Blazor WebAssembly project containing the interactive chatbot widget and its supporting services/models
- **`cgca-chatbot-worker`** — Cloudflare Worker that proxies chat requests to the Anthropic Claude API and handles lead capture

```
cgca.new/
├── cgca.sln                          # Solution file
├── cgca.web/                         # Server project (SSR + Interactive Server host)
│   ├── Program.cs                    # ASP.NET Core host — middleware, service registration, DB migration
│   ├── App.razor                     # Root HTML document — meta tags, structured data, GA4, CDN refs
│   ├── Routes.razor                  # Blazor router with NotFound fallback
│   ├── _Imports.razor                # Global using statements
│   ├── Data/
│   │   └── AppDbContext.cs           # EF Core DbContext (Identity + submissions + replies/notes)
│   ├── Migrations/                   # EF Core migration history
│   ├── Models/
│   │   ├── AdminUser.cs              # ASP.NET Identity user with DisplayName
│   │   ├── ContactSubmission.cs      # Contact form submission (read, acknowledged, replies)
│   │   ├── ContactReply.cs           # Threaded reply to a contact submission
│   │   ├── SponsorshipSubmission.cs  # Sponsorship inquiry (read, acknowledged, lifecycle flags, notes)
│   │   └── SponsorshipNote.cs        # Internal note on a sponsorship inquiry
│   ├── Services/
│   │   ├── ContactSubmissionService.cs    # CRUD, search, replies, acknowledged
│   │   ├── SponsorshipSubmissionService.cs # CRUD, search, lifecycle status, notes
│   │   └── EmailService.cs               # AWS SES via MailKit — notifications, confirmations, replies, forwards
│   ├── Endpoints/
│   │   ├── AuthEndpoints.cs          # Minimal API login/logout endpoints
│   │   └── ExportEndpoint.cs         # CSV export for submissions
│   ├── Layout/
│   │   ├── MainLayout.razor          # Public page shell (nav, footer, social links, chatbot)
│   │   ├── AdminLayout.razor         # Admin shell with sidebar navigation
│   │   └── NavMenu.razor             # Collapsible public navigation bar
│   ├── Pages/                        # Routable page components
│   │   ├── Home.razor                # Landing page — hero, quick facts, benefits, ABCs, CTAs
│   │   ├── About.razor               # Mission, vision, philosophy, statement of faith, conduct
│   │   ├── Programs.razor            # K4/K5 program details and schedules
│   │   ├── Admissions.razor          # Enrollment steps, tuition/fees, program cards
│   │   ├── Parents.razor             # Parent resources, tuition details, portal link
│   │   ├── Contact.razor             # Contact info, Google Maps, newsletter, contact form
│   │   ├── Sponsors.razor            # Sponsorship inquiry form
│   │   ├── Calendar.razor            # Embedded Google Calendar
│   │   ├── Privacy.razor             # Privacy policy
│   │   └── Admin/                    # Admin section (Interactive Server, all require [Authorize])
│   │       ├── Login.razor           # Admin login page
│   │       ├── Dashboard.razor       # Submission counts and recent activity
│   │       ├── ContactSubmissions.razor       # Contact inquiry list with search/filter/pagination
│   │       ├── ContactSubmissionDetail.razor  # Contact detail — forward, reply thread, acknowledge
│   │       ├── SponsorshipSubmissions.razor   # Sponsorship list with ack. and lifecycle badges
│   │       ├── SponsorshipSubmissionDetail.razor # Sponsorship detail — lifecycle tracking, notes
│   │       ├── Users.razor           # Admin user management
│   │       ├── UserEdit.razor        # Create/edit admin users
│   │       └── Profile.razor         # Current user profile and password change
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
└── cgca.web.Tests/                   # Unit and integration test project
    ├── StubChatService.cs            # No-op chat service for component tests
    ├── StubSubmissionServices.cs     # No-op submission services for component tests
    ├── Components/
    │   ├── AppTests.cs               # Routes component and router tests
    │   ├── NavMenuTests.cs           # Navigation links, toggle, rendering
    │   └── MainLayoutTests.cs        # Layout structure and content rendering
    ├── Pages/
    │   └── PageRenderingTests.cs     # All public pages render without exceptions
    ├── Services/
    │   ├── SubmissionServiceTests.cs # ContactSubmissionService + SponsorshipSubmissionService
    │   └── EmailServiceTests.cs      # EmailService construction and configuration
    └── Integration/
        └── RoutingTests.cs           # Route resolution for all public routes
```

### Rendering Model

- **Static SSR** — All public pages are rendered on the server and delivered as complete HTML. This provides instant page loads and allows search engines to crawl all content without executing JavaScript.
- **Interactive Server** — All admin pages use `@rendermode InteractiveServer` for full Blazor interactivity (real-time UI updates, SignalR-backed state) while keeping auth and data access server-side.
- **Interactive WASM Island** — The chatbot widget (`ChatWidget.razor`) is the sole WASM component, rendered with `InteractiveWebAssemblyRenderMode(prerender: false)` to run client-side only.
- **Native Bootstrap modals** — Modals on About and Parents pages use Bootstrap 5 `data-bs-toggle`/`data-bs-target` attributes rather than Blazor interactive components, so they work without a client-side runtime.
- **Scoped CSS** — Both `cgca.web.styles.css` and `cgca.web.client.styles.css` must be loaded in `App.razor` for component styles to apply correctly.

### Data Layer

- **Database**: SQLite at `Data/cgca.db` (relative to `ContentRootPath`), auto-migrated on startup
- **ORM**: EF Core with ASP.NET Core Identity (`AppDbContext` extends `IdentityDbContext<AdminUser>`)
- **Models**:
  - `ContactSubmission` — stores contact form inquiries with `IsRead`, `IsAcknowledged`, and a `Replies` collection
  - `ContactReply` — threaded admin replies to a contact submission (cascade-deleted with parent)
  - `SponsorshipSubmission` — stores sponsorship inquiries with `IsRead`, `IsAcknowledged`, lifecycle flags (`IsContacted`, `IsConfirmed`, `IsAddedToSystem`, `IsDeclined`), and a `Notes` collection
  - `SponsorshipNote` — internal admin notes on a sponsorship inquiry (cascade-deleted with parent)
- **Email**: AWS SES via MailKit (`EmailService`) — admin notifications, confirmation emails to submitters, reply/forward for contact messages
- **Auth**: ASP.NET Identity cookie auth; login at `/admin/login`, 8-hour sliding expiration, HttpOnly + SameSite=Strict; all admin routes require `[Authorize]`
- **Config**: Real secrets go in `appsettings.Development.json` (local, gitignored) or `appsettings.Production.json` (server, gitignored). See `appsettings.Production.example.json` for required keys.

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

### Public Pages

| Route | Page | Description |
|---|---|---|
| `/` | Home | Hero banner, quick facts, "Why Choose CGCA" benefits, ABCs, enrollment CTA |
| `/about` | About Us | Mission statement, vision, philosophy, statement of faith, code of conduct (modals) |
| `/programs` | Programs | K4/K5 program details, schedules, curriculum overview |
| `/admissions` | Admissions | 3-step enrollment process, tuition & fees tables, program cards |
| `/parents` | Parents | Parent resources, tuition modal, school calendar link, parent portal |
| `/contact` | Contact | Address, phone, email, hours, Google Maps embed, contact form |
| `/sponsors` | Sponsors | Sponsorship inquiry form |
| `/calendar` | Calendar | Embedded Google Calendar |
| `/privacy` | Privacy | Privacy policy |

### Admin Pages (require login)

| Route | Page | Description |
|---|---|---|
| `/admin/login` | Login | Admin authentication |
| `/admin` | Dashboard | Submission counts and recent activity |
| `/admin/contact` | Contact Submissions | List with search, read/unread filter, pagination |
| `/admin/contact/{id}` | Contact Detail | Full message, acknowledge, forward, reply thread |
| `/admin/sponsorships` | Sponsorship Submissions | List with acknowledged indicator and lifecycle badge |
| `/admin/sponsorships/{id}` | Sponsorship Detail | Inquiry detail, lifecycle tracking checkboxes, internal notes |
| `/admin/users` | Users | Admin user list |
| `/admin/users/{id}` | User Edit | Create or edit an admin user |
| `/admin/profile` | Profile | Current user profile and password change |

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

The project includes **96 tests** across four categories built with **bUnit**, **xUnit**, and **FluentAssertions**.

### Test Coverage

#### 1. Component Tests
- **NavMenuTests**: Toggle functionality, navigation links, logo rendering
- **AppTests**: Routes component rendering, router verification, NotFound configuration
- **MainLayoutTests**: Layout rendering, NavMenu inclusion, body content, container verification

#### 2. Page Rendering Tests
- Verifies all public pages (Home, About, Programs, Admissions, Parents, Contact, Sponsors, Calendar, Privacy) render without exceptions

#### 3. Routing Tests
- Validates all public routes resolve correctly
- Confirms router configuration and route-to-component mapping

#### 4. Service Tests (`SubmissionServiceTests`)
- **ContactSubmissionService**: submit, read/unread, acknowledged, search/filter, reply thread, cascade delete
- **SponsorshipSubmissionService**: submit, read/unread, acknowledged, lifecycle status flags (`IsContacted`, `IsConfirmed`, `IsAddedToSystem`, `IsDeclined`), notes thread, cascade delete

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

# Start the dev server (HTTP: localhost:5297, HTTPS: localhost:7183)
cd cgca.web
dotnet run

# Add a new EF Core migration (run from cgca.web/)
dotnet ef migrations add <MigrationName>

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
