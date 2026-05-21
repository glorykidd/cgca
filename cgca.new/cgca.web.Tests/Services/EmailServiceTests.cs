using cgca.web.Models;
using cgca.web.Services;
using FluentAssertions;

namespace cgca.web.Tests.Services;

public class EmailServiceTests
{
    private static ContactSubmission MakeContact(string name = "Jane Doe", string email = "jane@example.com",
        string subject = "General Inquiry", string message = "Hello!", string? phone = null) => new()
    {
        Id = 1,
        Name = name,
        Email = email,
        Subject = subject,
        Message = message,
        Phone = phone,
        SubmittedAt = new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc)
    };

    private static SponsorshipSubmission MakeSponsorship(string contactName = "Bob Smith",
        string email = "bob@example.com", string tier = "Gold",
        string? businessName = "Acme Corp", string? phone = null, string? message = null) => new()
    {
        Id = 2,
        ContactName = contactName,
        BusinessName = businessName,
        Email = email,
        SponsorshipTier = tier,
        Phone = phone,
        Message = message,
        SubmittedAt = new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc)
    };

    // --- Contact admin notification ---

    [Fact]
    public void BuildContactAdminHtml_ContainsSubmitterName()
    {
        var html = EmailService.BuildContactAdminHtml(MakeContact(name: "Jane Doe"));
        html.Should().Contain("Jane Doe");
    }

    [Fact]
    public void BuildContactAdminHtml_ContainsSubject()
    {
        var html = EmailService.BuildContactAdminHtml(MakeContact(subject: "Admissions / Enrollment"));
        html.Should().Contain("Admissions / Enrollment");
    }

    [Fact]
    public void BuildContactAdminHtml_ContainsMessage()
    {
        var html = EmailService.BuildContactAdminHtml(MakeContact(message: "I have a question."));
        html.Should().Contain("I have a question.");
    }

    [Fact]
    public void BuildContactAdminHtml_ShowsDashWhenPhoneAbsent()
    {
        var html = EmailService.BuildContactAdminHtml(MakeContact(phone: null));
        html.Should().Contain("—");
    }

    [Fact]
    public void BuildContactAdminHtml_ShowsPhoneWhenProvided()
    {
        var html = EmailService.BuildContactAdminHtml(MakeContact(phone: "(502) 555-1234"));
        html.Should().Contain("(502) 555-1234");
    }

    [Fact]
    public void BuildContactAdminHtml_ContainsAdminLink()
    {
        var submission = MakeContact();
        var html = EmailService.BuildContactAdminHtml(submission);
        html.Should().Contain($"/admin/contact/{submission.Id}");
    }

    [Fact]
    public void BuildContactAdminHtml_HtmlEncodesUserInput()
    {
        var html = EmailService.BuildContactAdminHtml(MakeContact(name: "<script>alert('xss')</script>"));
        html.Should().NotContain("<script>");
        html.Should().Contain("&lt;script&gt;");
    }

    // --- Contact confirmation ---

    [Fact]
    public void BuildContactConfirmationHtml_AddressesSubmitterByName()
    {
        var html = EmailService.BuildContactConfirmationHtml(MakeContact(name: "Jane Doe"));
        html.Should().Contain("Dear Jane Doe");
    }

    [Fact]
    public void BuildContactConfirmationHtml_ContainsPhoneNumber()
    {
        var html = EmailService.BuildContactConfirmationHtml(MakeContact());
        html.Should().Contain("(502) 543-4101");
    }

    [Fact]
    public void BuildContactConfirmationHtml_HtmlEncodesName()
    {
        var html = EmailService.BuildContactConfirmationHtml(MakeContact(name: "<b>Hacker</b>"));
        html.Should().NotContain("<b>Hacker</b>");
        html.Should().Contain("&lt;b&gt;Hacker&lt;/b&gt;");
    }

    // --- Sponsorship admin notification ---

    [Fact]
    public void BuildSponsorshipAdminHtml_ContainsContactName()
    {
        var html = EmailService.BuildSponsorshipAdminHtml(MakeSponsorship(contactName: "Bob Smith"));
        html.Should().Contain("Bob Smith");
    }

    [Fact]
    public void BuildSponsorshipAdminHtml_ContainsBusinessName()
    {
        var html = EmailService.BuildSponsorshipAdminHtml(MakeSponsorship(businessName: "Acme Corp"));
        html.Should().Contain("Acme Corp");
    }

    [Fact]
    public void BuildSponsorshipAdminHtml_HandlesNullBusinessName()
    {
        var html = EmailService.BuildSponsorshipAdminHtml(MakeSponsorship(businessName: null));
        html.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BuildSponsorshipAdminHtml_ContainsTier()
    {
        var html = EmailService.BuildSponsorshipAdminHtml(MakeSponsorship(tier: "Silver"));
        html.Should().Contain("Silver");
    }

    [Fact]
    public void BuildSponsorshipAdminHtml_ShowsNoneWhenMessageAbsent()
    {
        var html = EmailService.BuildSponsorshipAdminHtml(MakeSponsorship(message: null));
        html.Should().Contain("None");
    }

    [Fact]
    public void BuildSponsorshipAdminHtml_ShowsMessageWhenProvided()
    {
        var html = EmailService.BuildSponsorshipAdminHtml(MakeSponsorship(message: "Looking forward to it!"));
        html.Should().Contain("Looking forward to it!");
    }

    [Fact]
    public void BuildSponsorshipAdminHtml_ContainsAdminLink()
    {
        var submission = MakeSponsorship();
        var html = EmailService.BuildSponsorshipAdminHtml(submission);
        html.Should().Contain($"/admin/sponsorships/{submission.Id}");
    }

    [Fact]
    public void BuildSponsorshipAdminHtml_HtmlEncodesUserInput()
    {
        var html = EmailService.BuildSponsorshipAdminHtml(MakeSponsorship(contactName: "<script>xss</script>"));
        html.Should().NotContain("<script>xss</script>");
        html.Should().Contain("&lt;script&gt;");
    }

    // --- Contact reply ---

    [Fact]
    public void BuildContactReplyHtml_AddressesSubmitterByName()
    {
        var html = EmailService.BuildContactReplyHtml(MakeContact(name: "Jane Doe"), "Here is our response.");
        html.Should().Contain("Dear Jane Doe");
    }

    [Fact]
    public void BuildContactReplyHtml_ContainsReplyMessage()
    {
        var html = EmailService.BuildContactReplyHtml(MakeContact(), "We will follow up shortly.");
        html.Should().Contain("We will follow up shortly.");
    }

    [Fact]
    public void BuildContactReplyHtml_QuotesOriginalSubject()
    {
        var html = EmailService.BuildContactReplyHtml(MakeContact(subject: "Admissions Question"), "Reply here.");
        html.Should().Contain("Admissions Question");
    }

    [Fact]
    public void BuildContactReplyHtml_QuotesOriginalMessage()
    {
        var html = EmailService.BuildContactReplyHtml(MakeContact(message: "Original message text."), "Reply here.");
        html.Should().Contain("Original message text.");
    }

    [Fact]
    public void BuildContactReplyHtml_HtmlEncodesReplyMessage()
    {
        var html = EmailService.BuildContactReplyHtml(MakeContact(), "<script>alert('xss')</script>");
        html.Should().NotContain("<script>");
        html.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void BuildContactReplyHtml_HtmlEncodesOriginalMessage()
    {
        var html = EmailService.BuildContactReplyHtml(MakeContact(message: "<b>evil</b>"), "reply");
        html.Should().NotContain("<b>evil</b>");
        html.Should().Contain("&lt;b&gt;evil&lt;/b&gt;");
    }

    // --- Sponsorship confirmation ---

    [Fact]
    public void BuildSponsorshipConfirmationHtml_AddressesContactByName()
    {
        var html = EmailService.BuildSponsorshipConfirmationHtml(MakeSponsorship(contactName: "Bob Smith"));
        html.Should().Contain("Dear Bob Smith");
    }

    [Fact]
    public void BuildSponsorshipConfirmationHtml_ContainsTier()
    {
        var html = EmailService.BuildSponsorshipConfirmationHtml(MakeSponsorship(tier: "Bronze"));
        html.Should().Contain("Bronze Sponsor");
    }

    [Fact]
    public void BuildSponsorshipConfirmationHtml_ContainsPhoneNumber()
    {
        var html = EmailService.BuildSponsorshipConfirmationHtml(MakeSponsorship());
        html.Should().Contain("(502) 543-4101");
    }

    [Fact]
    public void BuildSponsorshipConfirmationHtml_HtmlEncodesName()
    {
        var html = EmailService.BuildSponsorshipConfirmationHtml(MakeSponsorship(contactName: "<b>Bad</b>"));
        html.Should().NotContain("<b>Bad</b>");
        html.Should().Contain("&lt;b&gt;Bad&lt;/b&gt;");
    }
}
