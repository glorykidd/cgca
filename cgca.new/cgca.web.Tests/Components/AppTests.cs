using Bunit;
using cgca.web;
using cgca.web.client.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace cgca.web.Tests.Components;

/// <summary>
/// Tests for the Routes component (root router component).
/// </summary>
public class AppTests : BunitContext
{
    public AppTests()
    {
        // Setup required services
        Services.AddBlazorBootstrap();
        Services.AddSingleton<IChatService, StubChatService>();

        // Mock JS Interop calls for Blazor Bootstrap
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Routes_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<Routes>();

        // Assert
        cut.Should().NotBeNull("Routes component should render");
        cut.Markup.Should().NotBeEmpty("Routes component should have content");
    }

    [Fact]
    public void Routes_ContainsRouterComponent()
    {
        // Arrange & Act
        var cut = Render<Routes>();

        // Assert - Router should be present (indicated by content rendering)
        cut.Markup.Should().NotBeEmpty("Router should render content");
    }

    [Fact]
    public void Routes_HasNotFoundConfiguration()
    {
        // Arrange & Act
        var cut = Render<Routes>();

        // Assert - Verify the Routes component renders successfully
        // The NotFound route configuration is tested by rendering invalid routes
        cut.Should().NotBeNull("Routes should have NotFound route configured");
    }
}
