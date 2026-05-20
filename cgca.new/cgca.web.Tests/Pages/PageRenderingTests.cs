using Bunit;
using cgca.web.Pages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace cgca.web.Tests.Pages;

/// <summary>
/// Tests to ensure all pages render without exceptions.
/// </summary>
public class PageRenderingTests : BunitContext
{
    public PageRenderingTests()
    {
        Services.AddBlazorBootstrap();
        Services.AddSingleton<cgca.web.Services.ContactSubmissionService, StubContactSubmissionService>();
        Services.AddSingleton<cgca.web.Services.SponsorshipSubmissionService, StubSponsorshipSubmissionService>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void HomePage_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<Home>();

        // Assert
        cut.Should().NotBeNull("Home page should render");
        cut.Markup.Should().NotBeEmpty("Home page should have content");
    }

    [Fact]
    public void AboutPage_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<About>();

        // Assert
        cut.Should().NotBeNull("About page should render");
        cut.Markup.Should().NotBeEmpty("About page should have content");
    }

    [Fact]
    public void ParentsPage_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<Parents>();

        // Assert
        cut.Should().NotBeNull("Parents page should render");
        cut.Markup.Should().NotBeEmpty("Parents page should have content");
    }

    [Fact]
    public void ContactPage_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<Contact>();

        // Assert
        cut.Should().NotBeNull("Contact page should render");
        cut.Markup.Should().NotBeEmpty("Contact page should have content");
    }

    [Fact]
    public void CalendarPage_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<Calendar>();

        // Assert
        cut.Should().NotBeNull("Calendar page should render");
        cut.Markup.Should().NotBeEmpty("Calendar page should have content");
    }

    [Fact]
    public void PrivacyPage_RendersWithoutException()
    {
        // Arrange & Act
        var cut = Render<Privacy>();

        // Assert
        cut.Should().NotBeNull("Privacy page should render");
        cut.Markup.Should().NotBeEmpty("Privacy page should have content");
    }

    [Fact]
    public void SponsorsPage_RendersWithoutException()
    {
        var cut = Render<Sponsors>();

        cut.Should().NotBeNull("Sponsors page should render");
        cut.Markup.Should().NotBeEmpty("Sponsors page should have content");
    }

    [Fact]
    public void AllPages_RenderWithoutExceptions()
    {
        Action renderHome = () => Render<Home>();
        Action renderAbout = () => Render<About>();
        Action renderParents = () => Render<Parents>();
        Action renderContact = () => Render<Contact>();
        Action renderCalendar = () => Render<Calendar>();
        Action renderPrivacy = () => Render<Privacy>();
        Action renderSponsors = () => Render<Sponsors>();

        renderHome.Should().NotThrow("Home page should render without exceptions");
        renderAbout.Should().NotThrow("About page should render without exceptions");
        renderParents.Should().NotThrow("Parents page should render without exceptions");
        renderContact.Should().NotThrow("Contact page should render without exceptions");
        renderCalendar.Should().NotThrow("Calendar page should render without exceptions");
        renderPrivacy.Should().NotThrow("Privacy page should render without exceptions");
        renderSponsors.Should().NotThrow("Sponsors page should render without exceptions");
    }
}
