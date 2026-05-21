namespace cgca.web.Models;

public class ContactSubmission
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime SubmittedAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsAcknowledged { get; set; }
    public List<ContactReply> Replies { get; set; } = [];
}
