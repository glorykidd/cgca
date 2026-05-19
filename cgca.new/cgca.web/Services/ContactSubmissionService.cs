using cgca.web.Data;
using cgca.web.Models;
using Microsoft.EntityFrameworkCore;

namespace cgca.web.Services;

public class ContactSubmissionService
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;

    public ContactSubmissionService(AppDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    public async Task<bool> SubmitAsync(ContactSubmission submission)
    {
        submission.SubmittedAt = DateTime.UtcNow;
        submission.IsRead = false;

        _db.ContactSubmissions.Add(submission);
        var saved = await _db.SaveChangesAsync() > 0;

        if (saved)
        {
            await _email.SendContactAdminNotificationAsync(submission);
            await _email.SendContactConfirmationAsync(submission);
        }

        return saved;
    }

    public async Task<ContactSubmission?> GetByIdAsync(int id) =>
        await _db.ContactSubmissions.FindAsync(id);

    public async Task<(List<ContactSubmission> Items, int TotalCount)> SearchAsync(
        string? searchTerm, bool? isReadFilter, int page, int pageSize)
    {
        var query = _db.ContactSubmissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.ToLower().Contains(term)));
        }

        if (isReadFilter.HasValue)
            query = query.Where(c => c.IsRead == isReadFilter.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task MarkAsReadAsync(int id)
    {
        var item = await _db.ContactSubmissions.FindAsync(id);
        if (item != null) { item.IsRead = true; await _db.SaveChangesAsync(); }
    }

    public async Task ToggleReadAsync(int id)
    {
        var item = await _db.ContactSubmissions.FindAsync(id);
        if (item != null) { item.IsRead = !item.IsRead; await _db.SaveChangesAsync(); }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _db.ContactSubmissions.FindAsync(id);
        if (item == null) return false;
        _db.ContactSubmissions.Remove(item);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<int> GetTotalCountAsync() => await _db.ContactSubmissions.CountAsync();
    public async Task<int> GetUnreadCountAsync() => await _db.ContactSubmissions.CountAsync(c => !c.IsRead);
    public async Task<int> GetCountSinceAsync(DateTime since) => await _db.ContactSubmissions.CountAsync(c => c.SubmittedAt >= since);
    public async Task<List<ContactSubmission>> GetAllAsync() => await _db.ContactSubmissions.OrderByDescending(c => c.SubmittedAt).ToListAsync();
}
