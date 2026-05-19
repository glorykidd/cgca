using System.Text;
using cgca.web.Services;

namespace cgca.web.Endpoints;

public static class ExportEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/admin/export/contacts", async (ContactSubmissionService service) =>
        {
            var items = await service.GetAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Email,Phone,Subject,Message,SubmittedAt,IsRead");

            foreach (var c in items)
                sb.AppendLine($"{c.Id},{Escape(c.Name)},{Escape(c.Email)},{Escape(c.Phone)},{Escape(c.Subject)},{Escape(c.Message)},{c.SubmittedAt:yyyy-MM-dd HH:mm:ss},{c.IsRead}");

            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"contact-submissions-{DateTime.UtcNow:yyyyMMdd}.csv");
        }).RequireAuthorization();

        app.MapGet("/admin/export/sponsorships", async (SponsorshipSubmissionService service) =>
        {
            var items = await service.GetAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,ContactName,BusinessName,Email,Phone,SponsorshipTier,Message,SubmittedAt,IsRead");

            foreach (var s in items)
                sb.AppendLine($"{s.Id},{Escape(s.ContactName)},{Escape(s.BusinessName)},{Escape(s.Email)},{Escape(s.Phone)},{Escape(s.SponsorshipTier)},{Escape(s.Message)},{s.SubmittedAt:yyyy-MM-dd HH:mm:ss},{s.IsRead}");

            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"sponsorship-submissions-{DateTime.UtcNow:yyyyMMdd}.csv");
        }).RequireAuthorization();
    }

    internal static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
