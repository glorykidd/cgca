using Bunit;
using cgca.web.Layout;
using FluentAssertions;
using Xunit;

namespace cgca.web.Tests.Components;

/// <summary>
/// Tests for the NavMenu component.
/// </summary>
public class NavMenuTests : BunitContext
{
    public NavMenuTests()
    {
        // Mock JS Interop calls for Blazor Bootstrap
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void NavMenu_InitiallyRenders_WithCollapsedState()
    {
        // Arrange & Act
        var cut = Render<NavMenu>();

        // Assert
        var navbarCollapse = cut.Find(".navbar-collapse");
        navbarCollapse.ClassList.Should().NotContain("show", "navbar should be collapsed initially");

        var toggleButton = cut.Find(".navbar-toggler");
        toggleButton.ClassList.Should().Contain("collapsed", "toggle button should show collapsed state");
    }

    [Fact]
    public void NavMenu_ToggleButton_ExpandsMenu()
    {
        // Arrange
        var cut = Render<NavMenu>();
        var toggleButton = cut.Find(".navbar-toggler");

        // Act - Click the toggle button
        toggleButton.Click();

        // Assert
        var navbarCollapse = cut.Find(".navbar-collapse");
        navbarCollapse.ClassList.Should().Contain("show", "navbar should be expanded after toggle");

        toggleButton.ClassList.Should().NotContain("collapsed", "toggle button should not show collapsed state");
    }

    [Fact]
    public void NavMenu_ToggleButton_CollapsesMenu_WhenClickedTwice()
    {
        // Arrange
        var cut = Render<NavMenu>();
        var toggleButton = cut.Find(".navbar-toggler");

        // Act - Click twice to expand then collapse
        toggleButton.Click();
        toggleButton.Click();

        // Assert
        var navbarCollapse = cut.Find(".navbar-collapse");
        navbarCollapse.ClassList.Should().NotContain("show", "navbar should be collapsed after second toggle");

        toggleButton.ClassList.Should().Contain("collapsed", "toggle button should show collapsed state again");
    }

    [Fact]
    public void NavMenu_RendersAllNavigationLinks()
    {
        // Arrange & Act
        var cut = Render<NavMenu>();

        // Assert - Check all navigation links are present
        cut.FindAll("a[href='']").Should().NotBeEmpty("Home link should be present");
        cut.FindAll("a[href='/about']").Should().NotBeEmpty("About link should be present");
        cut.FindAll("a[href='/programs']").Should().NotBeEmpty("Programs link should be present");
        cut.FindAll("a[href='/admissions']").Should().NotBeEmpty("Admissions link should be present");
        cut.FindAll("a[href='/parents']").Should().NotBeEmpty("Parents link should be present");
        cut.FindAll("a[href='/calendar']").Should().NotBeEmpty("Calendar link should be present");
        cut.FindAll("a[href='/contact']").Should().NotBeEmpty("Contact link should be present");
    }

    [Fact]
    public void NavMenu_Logo_IsRendered()
    {
        // Arrange & Act
        var cut = Render<NavMenu>();

        // Assert
        var logo = cut.Find("img[src='images/cgca-logo.png']");
        logo.Should().NotBeNull("logo image should be rendered");
        logo.ClassList.Should().Contain("image-logo");
    }

    [Fact]
    public void NavMenu_ClickingNavbarCollapse_TogglesMenu()
    {
        // Arrange
        var cut = Render<NavMenu>();
        var navbarCollapse = cut.Find(".navbar-collapse");

        // Act - Click the navbar collapse area itself
        navbarCollapse.Click();

        // Assert - Should toggle the menu
        navbarCollapse.ClassList.Should().Contain("show", "clicking navbar collapse should expand menu");
    }
}
