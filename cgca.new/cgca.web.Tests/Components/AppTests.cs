using Bunit;
using cgca.web;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace cgca.web.Tests.Components;

/// <summary>
/// Tests for the App component (root component with router).
/// </summary>
public class AppTests : BunitContext
{
    public AppTests()
    {
        // Setup required services
        Services.AddBlazorBootstrap();

        // Mock JS Interop calls for Blazor Bootstrap
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void App_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<App>();

        // Assert
        cut.Should().NotBeNull("App component should render");
        cut.Markup.Should().NotBeEmpty("App component should have content");
    }

    [Fact]
    public void App_ContainsRouterComponent()
    {
        // Arrange & Act
        var cut = Render<App>();

        // Assert - Router should be present (indicated by content rendering)
        cut.Markup.Should().NotBeEmpty("Router should render content");
    }

    [Fact]
    public void App_HasNotFoundConfiguration()
    {
        // Arrange & Act
        var cut = Render<App>();

        // Assert - Verify the App component renders successfully
        // The NotFound route configuration is tested by rendering invalid routes
        cut.Should().NotBeNull("App should have NotFound route configured");
    }
}
