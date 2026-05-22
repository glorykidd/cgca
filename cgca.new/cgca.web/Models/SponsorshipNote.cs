namespace cgca.web.Models;

public class SponsorshipNote
{
    public int Id { get; set; }
    public int SponsorshipSubmissionId { get; set; }
    public SponsorshipSubmission SponsorshipSubmission { get; set; } = default!;
    public string Message { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
