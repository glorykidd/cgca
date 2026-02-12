# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview
- Cedar Grove Christian Academy (CGCA) website — a Blazor WebAssembly client-side application targeting .NET 10.0
- Primary users: prospective and current parents, students, and school staff
- Integrates with Google Calendar (embedded schedule), Google Forms (contact form), SermonAudio (embedded sermons), and Praxis School (registration links)
- Static/semi-dynamic school website: informational pages, event calendar, contact, and parent resources

## Architecture & Patterns
- All source lives under `cgca.new/cgca.web/`; the solution file is `cgca.new/cgca.sln`
- **Entry point:** `Program.cs` — configures the WASM host, registers `App` as root component, adds `HttpClient` and Blazor Bootstrap services
- **Routing:** `App.razor` uses `<Router>` with `<RouteView>` and a `<NotFound>` fallback, wrapped in `MainLayout`
- **Layout:** `Layout/MainLayout.razor` (shared page shell: header, footer, social links) and `Layout/NavMenu.razor` (collapsible nav bar)
- **Pages:** Six routable pages under `Pages/` — `Home`, `About`, `Parents`, `Contact`, `Calendar`, `Privacy`. These are content-heavy Razor components with embedded HTML/Bootstrap markup
- **Styling:** Component-scoped CSS (`.razor.css` co-located files) plus a global stylesheet at `wwwroot/css/app.css`. Bootstrap 5.3 grid loaded via CDN in `wwwroot/index.html`
- **Static assets:** `wwwroot/` contains images, CSS, and the SPA entry point `index.html` which loads CDN dependencies (Bootstrap, Bootstrap Icons, Chart.js, Blazor Bootstrap JS)
- Third-party content is embedded via iframes (Google Calendar, Google Forms, SermonAudio)

## Stack Best Practices
- Blazor WebAssembly (client-side only, no server component)
- C# with nullable reference types and implicit usings enabled (.NET 10.0)
- Razor component model: each page/layout is a `.razor` file with optional scoped `.razor.css`
- Services registered via DI in `Program.cs` (e.g., `HttpClient`, Blazor Bootstrap)
- CDN dependencies declared in `wwwroot/index.html` — keep JS/CSS references there, not in individual components

## Anti-Patterns
- Do not hard-code secrets or API keys in Razor components or static files
- Do not inline large CSS blocks in `.razor` files — use scoped `.razor.css` or `wwwroot/css/app.css`
- Do not add server-side dependencies — this is a purely client-side WASM app

## Data Models
- No database or server-side data layer; all content is static HTML within Razor components
- External data comes from embedded third-party services (Google, SermonAudio)

## Security & Configuration
- Google Analytics (GA4) tracking ID is configured in `wwwroot/index.html`
- No authentication or authorization flow — public-facing informational site
- Dev server ports configured in `Properties/launchSettings.json` (HTTP: 5297, HTTPS: 7183)

## Commands & Scripts
All commands run from `cgca.new/cgca.web`:
```bash
# Build
dotnet build

# Run dev server (HTTP on localhost:5297, HTTPS on localhost:7183)
dotnet run

# Publish release build
dotnet publish -c Release -o <output-dir>
```
- No test project, linter, or formatter is configured in this repository
- CI/CD defined in `.github/workflows/cgca.yml`: triggers on push to `main` (path `cgca.new/**`) or manual dispatch; runs on a self-hosted Windows runner; backs up existing deployment then publishes via `dotnet publish -c Release`
- Branch strategy: `main` is production (triggers deploy), `develop` is the primary development branch (use as PR base)
