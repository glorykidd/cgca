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
    public bool IsAcknowledged { get; set; }
    public bool IsContacted { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsAddedToSystem { get; set; }
    public bool IsDeclined { get; set; }
    public List<SponsorshipNote> Notes { get; set; } = [];
}
