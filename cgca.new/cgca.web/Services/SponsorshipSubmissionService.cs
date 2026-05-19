using cgca.web.Data;
using cgca.web.Models;
using Microsoft.EntityFrameworkCore;

namespace cgca.web.Services;

public class SponsorshipSubmissionService
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;

    public SponsorshipSubmissionService(AppDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    public async Task<bool> SubmitAsync(SponsorshipSubmission submission)
    {
        submission.SubmittedAt = DateTime.UtcNow;
        submission.IsRead = false;

        _db.SponsorshipSubmissions.Add(submission);
        var saved = await _db.SaveChangesAsync() > 0;

        if (saved)
        {
            await _email.SendSponsorshipAdminNotificationAsync(submission);
            await _email.SendSponsorshipConfirmationAsync(submission);
        }

        return saved;
    }

    public async Task<SponsorshipSubmission?> GetByIdAsync(int id) =>
        await _db.SponsorshipSubmissions.FindAsync(id);

    public async Task<(List<SponsorshipSubmission> Items, int TotalCount)> SearchAsync(
        string? searchTerm, bool? isReadFilter, int page, int pageSize)
    {
        var query = _db.SponsorshipSubmissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.ContactName.ToLower().Contains(term) ||
                s.BusinessName.ToLower().Contains(term) ||
                s.Email.ToLower().Contains(term) ||
                (s.Phone != null && s.Phone.ToLower().Contains(term)));
        }

        if (isReadFilter.HasValue)
            query = query.Where(s => s.IsRead == isReadFilter.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task MarkAsReadAsync(int id)
    {
        var item = await _db.SponsorshipSubmissions.FindAsync(id);
        if (item != null) { item.IsRead = true; await _db.SaveChangesAsync(); }
    }

    public async Task ToggleReadAsync(int id)
    {
        var item = await _db.SponsorshipSubmissions.FindAsync(id);
        if (item != null) { item.IsRead = !item.IsRead; await _db.SaveChangesAsync(); }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _db.SponsorshipSubmissions.FindAsync(id);
        if (item == null) return false;
        _db.SponsorshipSubmissions.Remove(item);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<int> GetTotalCountAsync() => await _db.SponsorshipSubmissions.CountAsync();
    public async Task<int> GetUnreadCountAsync() => await _db.SponsorshipSubmissions.CountAsync(s => !s.IsRead);
    public async Task<int> GetCountSinceAsync(DateTime since) => await _db.SponsorshipSubmissions.CountAsync(s => s.SubmittedAt >= since);
    public async Task<List<SponsorshipSubmission>> GetAllAsync() => await _db.SponsorshipSubmissions.OrderByDescending(s => s.SubmittedAt).ToListAsync();
}
