using cgca.web.Data;
using cgca.web.Models;
using cgca.web.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace cgca.web.Tests.Services;

public class SubmissionServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ContactSubmissionService _contactService;
    private readonly SponsorshipSubmissionService _sponsorshipService;

    public SubmissionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        // Use a no-op email service so tests don't need SMTP
        var noopEmail = new NoopEmailService();
        _contactService = new ContactSubmissionService(_db, noopEmail);
        _sponsorshipService = new SponsorshipSubmissionService(_db, noopEmail);
    }

    public void Dispose() => _db.Dispose();

    // --- ContactSubmissionService ---

    [Fact]
    public async Task ContactSubmit_SavesSubmissionAndReturnsTrue()
    {
        var submission = new ContactSubmission
        {
            Name = "Jane Doe", Email = "jane@example.com",
            Subject = "General Inquiry", Message = "Hello!"
        };

        var result = await _contactService.SubmitAsync(submission);

        result.Should().BeTrue();
        _db.ContactSubmissions.Should().HaveCount(1);
    }

    [Fact]
    public async Task ContactSubmit_SetsSubmittedAtAndIsReadFalse()
    {
        var before = DateTime.UtcNow;
        var submission = new ContactSubmission
        {
            Name = "Jane", Email = "jane@example.com",
            Subject = "Test", Message = "Hi"
        };

        await _contactService.SubmitAsync(submission);

        submission.SubmittedAt.Should().BeOnOrAfter(before);
        submission.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task ContactGetById_ReturnsCorrectSubmission()
    {
        var submission = new ContactSubmission
        {
            Name = "Alice", Email = "alice@example.com",
            Subject = "Test", Message = "Hi"
        };
        await _contactService.SubmitAsync(submission);

        var result = await _contactService.GetByIdAsync(submission.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task ContactGetById_ReturnsNullForUnknownId()
    {
        var result = await _contactService.GetByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ContactMarkAsRead_SetsIsReadTrue()
    {
        var submission = new ContactSubmission
        {
            Name = "Bob", Email = "bob@example.com",
            Subject = "Test", Message = "Hi"
        };
        await _contactService.SubmitAsync(submission);

        await _contactService.MarkAsReadAsync(submission.Id);

        var updated = await _contactService.GetByIdAsync(submission.Id);
        updated!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task ContactToggleRead_FlipsIsRead()
    {
        var submission = new ContactSubmission
        {
            Name = "Carol", Email = "carol@example.com",
            Subject = "Test", Message = "Hi"
        };
        await _contactService.SubmitAsync(submission);

        await _contactService.ToggleReadAsync(submission.Id);
        (await _contactService.GetByIdAsync(submission.Id))!.IsRead.Should().BeTrue();

        await _contactService.ToggleReadAsync(submission.Id);
        (await _contactService.GetByIdAsync(submission.Id))!.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task ContactDelete_RemovesSubmissionAndReturnsTrue()
    {
        var submission = new ContactSubmission
        {
            Name = "Dave", Email = "dave@example.com",
            Subject = "Test", Message = "Hi"
        };
        await _contactService.SubmitAsync(submission);

        var result = await _contactService.DeleteAsync(submission.Id);

        result.Should().BeTrue();
        _db.ContactSubmissions.Should().BeEmpty();
    }

    [Fact]
    public async Task ContactDelete_ReturnsFalseForUnknownId()
    {
        var result = await _contactService.DeleteAsync(9999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ContactSearch_FiltersBySearchTerm()
    {
        await _contactService.SubmitAsync(new ContactSubmission { Name = "Alice", Email = "alice@example.com", Subject = "S", Message = "M" });
        await _contactService.SubmitAsync(new ContactSubmission { Name = "Bob", Email = "bob@example.com", Subject = "S", Message = "M" });

        var (items, total) = await _contactService.SearchAsync("alice", null, 1, 10);

        total.Should().Be(1);
        items.Single().Name.Should().Be("Alice");
    }

    [Fact]
    public async Task ContactSearch_FiltersByIsRead()
    {
        await _contactService.SubmitAsync(new ContactSubmission { Name = "Alice", Email = "a@a.com", Subject = "S", Message = "M" });
        var unread = new ContactSubmission { Name = "Bob", Email = "b@b.com", Subject = "S", Message = "M" };
        await _contactService.SubmitAsync(unread);
        await _contactService.MarkAsReadAsync(unread.Id);

        var (items, total) = await _contactService.SearchAsync(null, false, 1, 10);

        total.Should().Be(1);
        items.Single().Name.Should().Be("Alice");
    }

    [Fact]
    public async Task ContactGetUnreadCount_ReturnsOnlyUnread()
    {
        await _contactService.SubmitAsync(new ContactSubmission { Name = "A", Email = "a@a.com", Subject = "S", Message = "M" });
        var sub = new ContactSubmission { Name = "B", Email = "b@b.com", Subject = "S", Message = "M" };
        await _contactService.SubmitAsync(sub);
        await _contactService.MarkAsReadAsync(sub.Id);

        var count = await _contactService.GetUnreadCountAsync();

        count.Should().Be(1);
    }

    [Fact]
    public async Task ContactSetAcknowledged_SetsFlag()
    {
        var submission = new ContactSubmission { Name = "Eve", Email = "eve@example.com", Subject = "S", Message = "M" };
        await _contactService.SubmitAsync(submission);

        await _contactService.SetAcknowledgedAsync(submission.Id, true);

        (await _contactService.GetByIdAsync(submission.Id))!.IsAcknowledged.Should().BeTrue();
    }

    [Fact]
    public async Task ContactSetAcknowledged_ClearsFlag()
    {
        var submission = new ContactSubmission { Name = "Eve", Email = "eve@example.com", Subject = "S", Message = "M", IsAcknowledged = true };
        await _contactService.SubmitAsync(submission);

        await _contactService.SetAcknowledgedAsync(submission.Id, false);

        (await _contactService.GetByIdAsync(submission.Id))!.IsAcknowledged.Should().BeFalse();
    }

    [Fact]
    public async Task ContactAddReply_PersistsReply()
    {
        var submission = new ContactSubmission { Name = "Frank", Email = "frank@example.com", Subject = "S", Message = "M" };
        await _contactService.SubmitAsync(submission);

        var reply = await _contactService.AddReplyAsync(submission.Id, "Thank you for reaching out.", "Admin");

        reply.Id.Should().BeGreaterThan(0);
        reply.Message.Should().Be("Thank you for reaching out.");
        reply.SentBy.Should().Be("Admin");
        reply.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ContactGetByIdWithReplies_IncludesRepliesOrderedByTime()
    {
        var submission = new ContactSubmission { Name = "Grace", Email = "grace@example.com", Subject = "S", Message = "M" };
        await _contactService.SubmitAsync(submission);
        await _contactService.AddReplyAsync(submission.Id, "First reply", "Admin");
        await _contactService.AddReplyAsync(submission.Id, "Second reply", "Admin");

        var result = await _contactService.GetByIdWithRepliesAsync(submission.Id);

        result.Should().NotBeNull();
        result!.Replies.Should().HaveCount(2);
        result.Replies[0].Message.Should().Be("First reply");
        result.Replies[1].Message.Should().Be("Second reply");
    }

    [Fact]
    public async Task ContactDelete_CascadesReplies()
    {
        var submission = new ContactSubmission { Name = "Hank", Email = "hank@example.com", Subject = "S", Message = "M" };
        await _contactService.SubmitAsync(submission);
        await _contactService.AddReplyAsync(submission.Id, "A reply", "Admin");

        await _contactService.DeleteAsync(submission.Id);

        _db.ContactReplies.Should().BeEmpty();
    }

    // --- SponsorshipSubmissionService ---

    [Fact]
    public async Task SponsorshipSubmit_SavesSubmissionAndReturnsTrue()
    {
        var submission = new SponsorshipSubmission
        {
            ContactName = "Bob", Email = "bob@example.com", SponsorshipTier = "Gold"
        };

        var result = await _sponsorshipService.SubmitAsync(submission);

        result.Should().BeTrue();
        _db.SponsorshipSubmissions.Should().HaveCount(1);
    }

    [Fact]
    public async Task SponsorshipSubmit_SetsSubmittedAtAndIsReadFalse()
    {
        var before = DateTime.UtcNow;
        var submission = new SponsorshipSubmission
        {
            ContactName = "Bob", Email = "bob@example.com", SponsorshipTier = "Silver"
        };

        await _sponsorshipService.SubmitAsync(submission);

        submission.SubmittedAt.Should().BeOnOrAfter(before);
        submission.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task SponsorshipSubmit_WorksWithNullBusinessName()
    {
        var submission = new SponsorshipSubmission
        {
            ContactName = "Individual", Email = "i@example.com",
            SponsorshipTier = "Bronze", BusinessName = null
        };

        var result = await _sponsorshipService.SubmitAsync(submission);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SponsorshipDelete_RemovesAndReturnsTrue()
    {
        var submission = new SponsorshipSubmission
        {
            ContactName = "Bob", Email = "bob@example.com", SponsorshipTier = "Gold"
        };
        await _sponsorshipService.SubmitAsync(submission);

        var result = await _sponsorshipService.DeleteAsync(submission.Id);

        result.Should().BeTrue();
        _db.SponsorshipSubmissions.Should().BeEmpty();
    }

    [Fact]
    public async Task SponsorshipSearch_FiltersByBusinessName()
    {
        await _sponsorshipService.SubmitAsync(new SponsorshipSubmission { ContactName = "Alice", BusinessName = "Acme Corp", Email = "a@a.com", SponsorshipTier = "Gold" });
        await _sponsorshipService.SubmitAsync(new SponsorshipSubmission { ContactName = "Bob", BusinessName = null, Email = "b@b.com", SponsorshipTier = "Bronze" });

        var (items, total) = await _sponsorshipService.SearchAsync("acme", null, 1, 10);

        total.Should().Be(1);
        items.Single().ContactName.Should().Be("Alice");
    }

    [Fact]
    public async Task SponsorshipGetUnreadCount_ReturnsOnlyUnread()
    {
        await _sponsorshipService.SubmitAsync(new SponsorshipSubmission { ContactName = "A", Email = "a@a.com", SponsorshipTier = "Gold" });
        var sub = new SponsorshipSubmission { ContactName = "B", Email = "b@b.com", SponsorshipTier = "Silver" };
        await _sponsorshipService.SubmitAsync(sub);
        await _sponsorshipService.MarkAsReadAsync(sub.Id);

        var count = await _sponsorshipService.GetUnreadCountAsync();

        count.Should().Be(1);
    }
}

// Minimal no-op subclass so tests don't need real config or SMTP
file class NoopEmailService : EmailService
{
    public NoopEmailService() : base(null!, null!) { }

    public override Task SendContactAdminNotificationAsync(cgca.web.Models.ContactSubmission _) => Task.CompletedTask;
    public override Task SendContactConfirmationAsync(cgca.web.Models.ContactSubmission _) => Task.CompletedTask;
    public override Task SendContactReplyAsync(cgca.web.Models.ContactSubmission _, string __) => Task.CompletedTask;
    public override Task ForwardContactSubmissionAsync(cgca.web.Models.ContactSubmission _, string __) => Task.CompletedTask;
    public override Task SendSponsorshipAdminNotificationAsync(cgca.web.Models.SponsorshipSubmission _) => Task.CompletedTask;
    public override Task SendSponsorshipConfirmationAsync(cgca.web.Models.SponsorshipSubmission _) => Task.CompletedTask;
}
