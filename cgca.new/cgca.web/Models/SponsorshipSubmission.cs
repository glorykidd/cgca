namespace cgca.web.Models;

public class SponsorshipSubmission
{
    public int Id { get; set; }
    public string ContactName { get; set; } = "";
    public string? BusinessName { get; set; }
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string SponsorshipTier { get; set; } = "";
    public string? Message { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsRead { get; set; }
}
