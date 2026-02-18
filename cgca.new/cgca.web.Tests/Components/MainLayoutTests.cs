using Bunit;
using cgca.web.Layout;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace cgca.web.Tests.Components;

/// <summary>
/// Tests for the MainLayout component.
/// </summary>
public class MainLayoutTests : BunitContext
{
    public MainLayoutTests()
    {
        // Mock JS Interop calls for Blazor Bootstrap
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void MainLayout_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddMarkupContent(0, "<p>Test content</p>"))));

        // Assert
        cut.Should().NotBeNull("MainLayout should render");
        cut.Markup.Should().NotBeEmpty("MainLayout should have content");
    }

    [Fact]
    public void MainLayout_ContainsNavMenu()
    {
        // Arrange & Act
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddMarkupContent(0, "<p>Test content</p>"))));

        // Assert
        cut.FindAll(".navbar").Should().NotBeEmpty("MainLayout should contain NavMenu with navbar");
    }

    [Fact]
    public void MainLayout_RendersBodyContent()
    {
        // Arrange
        var testContent = "<div class='test-content'>Test Body Content</div>";

        // Act
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddMarkupContent(0, testContent))));

        // Assert
        cut.Markup.Should().Contain("Test Body Content", "Body content should be rendered");
    }

    [Fact]
    public void MainLayout_HasMainContentContainer()
    {
        // Arrange & Act
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddMarkupContent(0, "<p>Test</p>"))));

        // Assert
        cut.FindAll("main, .main, .container, .content").Should().NotBeEmpty(
            "MainLayout should have a main content container");
    }
}
