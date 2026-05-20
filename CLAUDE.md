# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview
- Cedar Grove Christian Academy (CGCA) website — an ASP.NET Core + Blazor hybrid application targeting .NET 10.0
- Primary users: prospective and current parents, students, and school staff
- Integrates with Google Calendar (embedded schedule), SermonAudio (embedded sermons), and Praxis School (registration links)
- Public-facing informational site (pages, event calendar, contact, sponsors) with a server-side admin section for managing contact and sponsorship submissions

## Architecture & Patterns
- All source lives under `cgca.new/cgca.web/`; the solution file is `cgca.new/cgca.sln`
- **Entry point:** `Program.cs` — configures ASP.NET Core host, registers Identity/EF Core/EmailService/submission services, seeds admin user, maps Razor components and API endpoints
- **Routing:** `App.razor` uses `<Router>` with `<RouteView>` and a `<NotFound>` fallback, wrapped in `MainLayout`
- **Layouts:** `Layout/MainLayout.razor` (public page shell) and `Layout/AdminLayout.razor` (admin shell with sidebar nav)
- **Pages:** Public pages under `Pages/` — `Home`, `About`, `Parents`, `Contact`, `Sponsors`, `Calendar`, `Privacy`. Admin pages under `Pages/Admin/` — `Dashboard`, `Login`, `ContactSubmissions`, `ContactSubmissionDetail`, `SponsorshipSubmissions`, `SponsorshipSubmissionDetail`, `Users`, `UserEdit`, `Profile`
- **Render modes:** Public pages use interactive WebAssembly render mode; admin pages use interactive Server render mode
- **Styling:** Component-scoped CSS (`.razor.css` co-located files) plus a global stylesheet at `wwwroot/css/app.css`. Bootstrap 5.3 loaded via CDN in `wwwroot/index.html`
- **Static assets:** `wwwroot/` contains images, CSS, and the SPA entry point `index.html` which loads CDN dependencies (Bootstrap, Bootstrap Icons, Chart.js, Blazor Bootstrap JS)
- Third-party content is embedded via iframes (Google Calendar, SermonAudio)

## Data Layer
- SQLite database at `Data/cgca.db` (relative to `ContentRootPath`), created and migrated automatically on startup
- EF Core with ASP.NET Identity — `AppDbContext` stores `AdminUser`, `ContactSubmission`, and `SponsorshipSubmission`
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
- Google Analytics (GA4) tracking ID is configured in `wwwroot/index.html`
- Dev server ports configured in `Properties/launchSettings.json` (HTTP: 5297, HTTPS: 7183)

## Stack Best Practices
- C# with nullable reference types and implicit usings enabled (.NET 10.0)
- Razor component model: each page/layout is a `.razor` file with optional scoped `.razor.css`
- Services registered via DI in `Program.cs`
- CDN dependencies declared in `wwwroot/index.html` — keep JS/CSS references there, not in individual components

## Anti-Patterns
- Do not hard-code secrets or API keys in Razor components, `appsettings.json`, or static files
- Do not inline large CSS blocks in `.razor` files — use scoped `.razor.css` or `wwwroot/css/app.css`
- Do not add `[Authorize]`-free routes under `Pages/Admin/` other than `Login`

## Commands & Scripts
All commands run from `cgca.new/cgca.web`:
```bash
# Build
dotnet build

# Run dev server (HTTP on localhost:5297, HTTPS on localhost:7183)
dotnet run

# Publish release build
dotnet publish -c Release -o <output-dir>

# Add a new EF Core migration
dotnet ef migrations add <MigrationName>
```
- No test project, linter, or formatter is configured in this repository
- CI/CD defined in `.github/workflows/cgca.yml`: triggers on push to `main` (path `cgca.new/**`) or manual dispatch; runs on a self-hosted Windows runner; backs up existing deployment then publishes via `dotnet publish -c Release`
- Branch strategy: `main` is production (triggers deploy), `develop` is the primary development branch (use as PR base)
