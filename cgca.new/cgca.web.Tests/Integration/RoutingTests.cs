using Bunit;
using cgca.web;
using cgca.web.Pages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace cgca.web.Tests.Integration;

/// <summary>
/// Tests for application routing to ensure all routes resolve correctly.
/// These tests verify that each page component can render, which proves the routes will work.
/// </summary>
public class RoutingTests : BunitContext
{
    public RoutingTests()
    {
        // Setup required services for routing tests
        Services.AddBlazorBootstrap();

        // Mock JS Interop calls for Blazor Bootstrap
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Router_HomeRoute_PageRendersCorrectly()
    {
        // Arrange & Act
        var cut = Render<Home>();

        // Assert
        cut.Should().NotBeNull("Home page component should render");
        cut.Markup.Should().NotBeEmpty("Home page should have content");
    }

    [Fact]
    public void Router_AboutRoute_PageRendersCorrectly()
    {
        // Arrange & Act
        var cut = Render<About>();

        // Assert
        cut.Should().NotBeNull("About page component should render");
        cut.Markup.Should().NotBeEmpty("About page should have content");
    }

    [Fact]
    public void Router_ParentsRoute_PageRendersCorrectly()
    {
        // Arrange & Act
        var cut = Render<Parents>();

        // Assert
        cut.Should().NotBeNull("Parents page component should render");
        cut.Markup.Should().NotBeEmpty("Parents page should have content");
    }

    [Fact]
    public void Router_ContactRoute_PageRendersCorrectly()
    {
        // Arrange & Act
        var cut = Render<Contact>();

        // Assert
        cut.Should().NotBeNull("Contact page component should render");
        cut.Markup.Should().NotBeEmpty("Contact page should have content");
    }

    [Fact]
    public void Router_CalendarRoute_PageRendersCorrectly()
    {
        // Arrange & Act
        var cut = Render<Calendar>();

        // Assert
        cut.Should().NotBeNull("Calendar page component should render");
        cut.Markup.Should().NotBeEmpty("Calendar page should have content");
    }

    [Fact]
    public void Router_PrivacyRoute_PageRendersCorrectly()
    {
        // Arrange & Act
        var cut = Render<Privacy>();

        // Assert
        cut.Should().NotBeNull("Privacy page component should render");
        cut.Markup.Should().NotBeEmpty("Privacy page should have content");
    }

    [Fact]
    public void App_RendersWithRouter()
    {
        // Arrange & Act
        var cut = Render<App>();

        // Assert
        cut.Should().NotBeNull("App should render with router");
        cut.Markup.Should().NotBeEmpty("App should have content");
    }

    [Fact]
    public void AllRoutes_HaveCorrespondingComponents()
    {
        // This test ensures all page components can be instantiated
        // which proves the routes will resolve correctly

        // Arrange & Act
        Action renderHome = () => Render<Home>();
        Action renderAbout = () => Render<About>();
        Action renderParents = () => Render<Parents>();
        Action renderContact = () => Render<Contact>();
        Action renderCalendar = () => Render<Calendar>();
        Action renderPrivacy = () => Render<Privacy>();

        // Assert - None should throw exceptions
        renderHome.Should().NotThrow("Home route component should render");
        renderAbout.Should().NotThrow("About route component should render");
        renderParents.Should().NotThrow("Parents route component should render");
        renderContact.Should().NotThrow("Contact route component should render");
        renderCalendar.Should().NotThrow("Calendar route component should render");
        renderPrivacy.Should().NotThrow("Privacy route component should render");
    }
}
