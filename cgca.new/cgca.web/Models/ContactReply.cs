namespace cgca.web.Models;

public class ContactReply
{
    public int Id { get; set; }
    public int ContactSubmissionId { get; set; }
    public ContactSubmission ContactSubmission { get; set; } = default!;
    public string Message { get; set; } = "";
    public string SentBy { get; set; } = "";
    public DateTime SentAt { get; set; }
}
