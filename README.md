# Cedar Grove Christian Academy Website

## Summary

The CGCA website is a client-side single-page application built with Blazor WebAssembly on .NET 10.0. It serves as the public-facing site for Cedar Grove Christian Academy, providing information about the school's mission, values, and programs to prospective and current families. The site includes pages for school information, parent resources, an embedded event calendar, a contact form, and a privacy policy. Third-party services — Google Calendar, Google Forms, SermonAudio, and Praxi School — are integrated via embedded content and external links.

## Technology Overview

| Layer | Technology |
|---|---|
| Framework | .NET 10.0 / ASP.NET Core Blazor WebAssembly |
| Language | C# (nullable reference types, implicit usings) |
| UI Components | Blazor Bootstrap 3.5.0 |
| CSS Framework | Bootstrap 5.3.7 (CDN) |
| Icons | Bootstrap Icons 1.13.1 (CDN) |
| Analytics | Google Analytics 4 |
| CI/CD | GitHub Actions — self-hosted Windows runner |
| License | MIT |

The application runs entirely in the browser via WebAssembly. There is no server-side rendering, no database, and no backend API. Content is static HTML within Razor components, supplemented by embedded third-party services.

## Architecture Overview

The application is a purely client-side Blazor WebAssembly SPA with no server-side backend. All source code lives under `cgca.new/cgca.web/`.

```
cgca.new/cgca.web/
├── Program.cs                # App entry point — WASM host, DI, service registration
├── App.razor                 # Root component — router with NotFound fallback
├── _Imports.razor            # Global using statements
├── Layout/
│   ├── MainLayout.razor      # Shared page shell (header, footer, social links)
│   └── NavMenu.razor         # Collapsible navigation bar
├── Pages/                    # Routable page components
│   ├── Home.razor            # Landing page — mission, values, sermon, registration CTAs
│   ├── About.razor           # Mission, vision, philosophy, core beliefs
│   ├── Parents.razor         # Parent resources and onboarding info
│   ├── Contact.razor         # Contact info with embedded Google Form
│   ├── Calendar.razor        # Embedded Google Calendar
│   └── Privacy.razor         # Privacy policy
├── Properties/
│   └── launchSettings.json   # Dev server configuration
└── wwwroot/                  # Static assets served to the browser
    ├── index.html            # SPA entry point — CDN references for Bootstrap, JS libs
    ├── css/app.css           # Global stylesheet
    ├── images/               # Site images
    └── lib/                  # Local library assets
```

Routing is handled by the Blazor `<Router>` in `App.razor`, which renders pages inside `MainLayout`. Styling uses a combination of a global stylesheet (`wwwroot/css/app.css`) and component-scoped CSS files (`.razor.css`) co-located with their components. All CDN dependencies (Bootstrap CSS/JS, Bootstrap Icons, Chart.js, Blazor Bootstrap) are loaded from `wwwroot/index.html`.

## Contribution Instructions

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### Getting Started

```bash
cd cgca.new/cgca.web
dotnet build
dotnet run
```

The dev server starts on **http://localhost:5297** and **https://localhost:7183**.

### Branching

- `main` — production branch; pushes trigger the deployment pipeline
- `develop` — primary development branch; use as the base for all pull requests

Create feature branches off `develop` and open PRs back into `develop`.

### Deployment

Deployments are automated via GitHub Actions (`.github/workflows/cgca.yml`). The workflow triggers on pushes to `main` that include changes under `cgca.new/`. It backs up the current production deployment, then publishes the new build to the production server.
