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
| Testing | xUnit, bUnit 2.5.3, FluentAssertions 8.8.0 |
| CI/CD | GitHub Actions — self-hosted Windows runner |
| License | MIT |

The application runs entirely in the browser via WebAssembly. There is no server-side rendering, no database, and no backend API. Content is static HTML within Razor components, supplemented by embedded third-party services.

## Architecture Overview

The application is a purely client-side Blazor WebAssembly SPA with no server-side backend. The solution consists of two projects under `cgca.new/`:

```
cgca.new/
├── cgca.sln                      # Solution file
├── cgca.web/                     # Main web application
│   ├── Program.cs                # App entry point — WASM host, DI, service registration
│   ├── App.razor                 # Root component — router with NotFound fallback
│   ├── _Imports.razor.           # Global using statements
│   ├── Layout/
│   │   ├── MainLayout.razor      # Shared page shell (header, footer, social links)
│   │   └── NavMenu.razor         # Collapsible navigation bar
│   ├── Pages/                    # Routable page components
│   │   ├── Home.razor            # Landing page — mission, values, sermon, registration CTAs
│   │   ├── About.razor           # Mission, vision, philosophy, core beliefs
│   │   ├── Parents.razor         # Parent resources and onboarding info
│   │   ├── Contact.razor         # Contact info with embedded Google Form
│   │   ├── Calendar.razor        # Embedded Google Calendar
│   │   └── Privacy.razor         # Privacy policy
│   ├── Properties/
│   │   └── launchSettings.json   # Dev server configuration
│   └── wwwroot/                  # Static assets served to the browser
│       ├── index.html            # SPA entry point — CDN references for Bootstrap, JS libs
│       ├── css/app.css           # Global stylesheet
│       ├── images/               # Site images
│       └── lib/                  # Local library assets
└── cgca.web.Tests/               # Unit test project
    ├── Components/               # Component unit tests
    │   ├── AppTests.cs           # App component and router tests
    │   ├── NavMenuTests.cs       # Navigation menu toggle and rendering tests
    │   └── MainLayoutTests.cs    # Layout component tests
    ├── Pages/                    # Page rendering tests
    │   └── PageRenderingTests.cs # Tests for all pages
    └── Integration/              # Integration tests
        └── RoutingTests.cs       # Route resolution tests
```

Routing is handled by the Blazor `<Router>` in `App.razor`, which renders pages inside `MainLayout`. Styling uses a combination of a global stylesheet (`wwwroot/css/app.css`) and component-scoped CSS files (`.razor.css`) co-located with their components. All CDN dependencies (Bootstrap CSS/JS, Bootstrap Icons, Chart.js, Blazor Bootstrap) are loaded from `wwwroot/index.html`.

## Unit Tests

The project includes a comprehensive test suite built with **bUnit**, **xUnit**, and **FluentAssertions** to ensure code quality and prevent regressions.

### Test Coverage

The test suite includes **30 tests** across three categories:

#### 1. Component Tests (12 tests)
- **NavMenuTests**: Toggle functionality, navigation links, external links, logo rendering
- **AppTests**: App component rendering, router verification, NotFound configuration
- **MainLayoutTests**: Layout rendering, NavMenu inclusion, body content, container verification

#### 2. Page Rendering Tests (7 tests)
- Verifies all pages (Home, About, Parents, Contact, Calendar, Privacy) render without exceptions
- Ensures comprehensive page functionality

#### 3. Routing Tests (8 tests)
- Validates all routes resolve correctly
- Confirms router configuration
- Tests route-to-component mapping

### Running Tests

```bash
# Run all tests
dotnet test cgca.new/cgca.sln

# Run tests with detailed output
dotnet test cgca.new/cgca.sln --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~NavMenuTests"

# Run tests in specific namespace
dotnet test --filter "FullyQualifiedName~Components"
```

### Test Framework Details

| Framework | Purpose |
|---|---|
| **xUnit** | Core test framework for organizing and running tests |
| **bUnit 2.5.3** | Blazor component testing library for rendering and interacting with components |
| **FluentAssertions 8.8.0** | Provides readable, expressive assertion syntax |

### CI/CD Integration

Tests are automatically executed during the GitHub Actions deployment workflow:
- Tests run immediately after code checkout
- Deployment only proceeds if all tests pass
- Test failures block the production deployment
- Detailed test output is logged for debugging

### Writing New Tests

When adding new components or features, follow these patterns:

1. **Component Tests**: Test interactive behavior, state changes, and rendering
2. **Page Tests**: Ensure pages render without exceptions and contain expected content
3. **Routing Tests**: Verify new routes resolve correctly

Example test structure:
```csharp
public class MyComponentTests : BunitContext
{
    public MyComponentTests()
    {
        // Setup services and mock JS Interop
        Services.AddBlazorBootstrap();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void MyComponent_RendersCorrectly()
    {
        // Arrange & Act
        var cut = Render<MyComponent>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeEmpty();
    }
}
```

## Contribution Instructions

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### Getting Started

```bash
# Build the solution
cd cgca.new
dotnet build

# Run tests
dotnet test

# Start the dev server
cd cgca.web
dotnet run
```

The dev server starts on **http://localhost:5297** and **https://localhost:7183**.

### Branching

- `main` — production branch; pushes trigger the deployment pipeline
- `develop` — primary development branch; use as the base for all pull requests

Create feature branches off `develop` and open PRs back into `develop`.

### Deployment

Deployments are automated via GitHub Actions (`.github/workflows/cgca.yml`). The workflow triggers on pushes to `main` that include changes under `cgca.new/`.

**Deployment Pipeline:**
1. Code checkout from repository
2. **Run unit tests** — deployment aborts if tests fail
3. Generate build number (timestamp)
4. Create backup of current production deployment
5. Build and publish to production server

All tests must pass before deployment proceeds, ensuring production stability.
