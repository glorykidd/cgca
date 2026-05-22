using cgca.web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace cgca.web.Data;

public class AppDbContext : IdentityDbContext<AdminUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();
    public DbSet<SponsorshipSubmission> SponsorshipSubmissions => Set<SponsorshipSubmission>();
    public DbSet<ContactReply> ContactReplies => Set<ContactReply>();
    public DbSet<SponsorshipNote> SponsorshipNotes => Set<SponsorshipNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContactSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SubmittedAt).IsDescending();
            entity.HasIndex(e => e.IsRead);
        });

        modelBuilder.Entity<SponsorshipSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SubmittedAt).IsDescending();
            entity.HasIndex(e => e.IsRead);
        });

        modelBuilder.Entity<ContactReply>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContactSubmissionId);
            entity.HasOne(e => e.ContactSubmission)
                  .WithMany(e => e.Replies)
                  .HasForeignKey(e => e.ContactSubmissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SponsorshipNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SponsorshipSubmissionId);
            entity.HasOne(e => e.SponsorshipSubmission)
                  .WithMany(e => e.Notes)
                  .HasForeignKey(e => e.SponsorshipSubmissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
