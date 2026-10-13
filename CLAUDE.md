# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview
- Cedar Grove Christian Academy (CGCA) website — an ASP.NET Core + Blazor hybrid application targeting .NET 10.0
- Primary users: prospective and current parents, students, and school staff
- Integrates with Google Calendar (embedded schedule), SermonAudio (embedded sermons), and Praxis School (registration links)
- Includes an AI chatbot ("Gracie") — a Blazor WebAssembly widget backed by a Cloudflare Worker that proxies to the Claude API
- Public-facing informational site (pages, event calendar, contact, sponsors) with a server-side admin section for managing contact and sponsorship submissions

## Architecture & Patterns
- Solution file is `cgca.new/cgca.sln`, with three .NET projects: `cgca.web` (server), `cgca.web.client` (chatbot WASM island), and `cgca.web.Tests` (xUnit/bUnit test suite). A fourth repo component, `cgca-chatbot-worker`, is a Cloudflare Worker (JavaScript) and is not part of the solution
- Public-site source lives under `cgca.new/cgca.web/`
- **Entry point:** `Program.cs` — configures ASP.NET Core host, registers Identity/EF Core/EmailService/submission services, seeds admin user, maps Razor components and API endpoints
- **Routing:** `App.razor` is the root HTML document (meta tags, structured data, GA4, CDN refs); `Routes.razor` holds the `<Router>` with `<RouteView>` and a `<NotFound>` fallback, wrapped in `MainLayout`
- **Layouts:** `Layout/MainLayout.razor` (public page shell) and `Layout/AdminLayout.razor` (admin shell with sidebar nav)
- **Pages:** Public pages under `Pages/` — `Home`, `About`, `Programs`, `Admissions`, `Parents`, `Contact`, `Sponsors`, `Calendar`, `Privacy`. Admin pages under `Pages/Admin/` — `Dashboard`, `Login`, `ContactSubmissions`, `ContactSubmissionDetail`, `SponsorshipSubmissions`, `SponsorshipSubmissionDetail`, `Users`, `UserEdit`, `Profile`
- **Render modes:** Public pages are statically server-rendered (SSR) by default; pages with forms (`Contact`, `Sponsors`) and all admin pages use `@rendermode InteractiveServer`. The chatbot widget (`cgca.web.client/Components/ChatWidget.razor`) is the sole `InteractiveWebAssembly` component
- **Styling:** Component-scoped CSS (`.razor.css` co-located files) plus a global stylesheet at `wwwroot/css/app.css`. Bootstrap 5.3, Bootstrap Icons, and Blazor Bootstrap are loaded via CDN/static assets in `App.razor` (there is no `wwwroot/index.html` — this is a Blazor Web App, not a hosted WASM SPA)
- **Static assets:** `wwwroot/` contains images, CSS, `robots.txt`, and `sitemap.xml`
- Third-party content is embedded via iframes (Google Calendar, SermonAudio)
- The chatbot (`cgca.web.client`) talks to the Cloudflare Worker in `cgca-chatbot-worker/src/` (`index.js`, `systemPrompt.js`, `rateLimit.js`, `cache.js`, `schoolInfo.js`), which proxies to the Anthropic Claude API and handles lead capture

## Data Layer
- SQLite database at `Data/cgca.db` (relative to `ContentRootPath`), created and migrated automatically on startup
- EF Core with ASP.NET Identity — `AppDbContext` stores `AdminUser`, `ContactSubmission` (with threaded `ContactReply` entries), and `SponsorshipSubmission` (with lifecycle flags and threaded `SponsorshipNote` entries)
- Migrations live under `Migrations/`; run `dotnet ef migrations add <Name>` to add new ones
- Services: `ContactSubmissionService`, `SponsorshipSubmissionService`, `EmailService` (AWS SES via MailKit)

## Security & Configuration
- ASP.NET Identity handles admin authentication; all `Pages/Admin/` pages (except `Login`) require `[Authorize]`
- Cookie auth: login path `/admin/login`, 8-hour sliding expiration, HttpOnly + SameSite=Strict
- API endpoints in `Endpoints/AuthEndpoints.cs` (login/logout) and `Endpoints/ExportEndpoint.cs`
- `appsettings.json` — committed with empty secrets (safe defaults); do not add real values here
- `appsettings.Development.json` — gitignored; use for local dev secrets
- `appsettings.Production.json` — gitignored; must be configured on the server before first run
- `appsettings.Production.example.json` — committed template showing required keys (`Email.*`, `AdminSeed.*`)
- `AdminSeed:Password` **must** be set in config — startup throws if it is missing or empty
- Google Analytics (GA4) tracking ID is configured in `App.razor`
- Dev server ports configured in `Properties/launchSettings.json` (HTTP: 5297, HTTPS: 7183)

## Stack Best Practices
- C# with nullable reference types and implicit usings enabled (.NET 10.0)
- Razor component model: each page/layout is a `.razor` file with optional scoped `.razor.css`
- Services registered via DI in `Program.cs`
- CDN dependencies declared in `App.razor` — keep JS/CSS references there, not in individual components
- Tests use xUnit, bUnit, and FluentAssertions — see `cgca.web.Tests/` (component rendering, routing, and service tests)

## Anti-Patterns
- Do not hard-code secrets or API keys in Razor components, `appsettings.json`, or static files
- Do not inline large CSS blocks in `.razor` files — use scoped `.razor.css` or `wwwroot/css/app.css`
- Do not add `[Authorize]`-free routes under `Pages/Admin/` other than `Login`

## Commands & Scripts
Build/run commands run from `cgca.new/cgca.web`; test and solution-wide commands run from `cgca.new/`:
```bash
# Build
dotnet build

# Run dev server (HTTP on localhost:5297, HTTPS on localhost:7183)
dotnet run

# Publish release build
dotnet publish -c Release -o <output-dir>

# Add a new EF Core migration
dotnet ef migrations add <MigrationName>

# Run the test suite (from cgca.new/)
dotnet test cgca.sln
```
- No linter or formatter is configured in this repository, but a test project (`cgca.web.Tests`, xUnit/bUnit/FluentAssertions) exists and is run in CI
- CI/CD workflows in `.github/workflows/`:
  - `cgca-develop.yml` — runs tests + build on push to `develop`
  - `cgca.yml` — triggers on push to `main` (path `cgca.new/**`) or manual dispatch; runs on a self-hosted Windows runner; runs tests, backs up the existing deployment, publishes via `dotnet publish -c Release`, then deploys the chatbot worker
  - `cgca-chatbot.yml` — redeploys the Cloudflare Worker on push to `main` when only `cgca-chatbot-worker/**` changes
  - `codeql.yml` — CodeQL security scanning
- Branch strategy: `main` is production (triggers deploy), `develop` is the primary development branch (use as PR base)
