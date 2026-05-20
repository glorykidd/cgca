using cgca.web.Models;
using cgca.web.Services;

namespace cgca.web.Tests;

public class StubContactSubmissionService : ContactSubmissionService
{
    public StubContactSubmissionService() : base(null!, null!) { }

    public override Task<bool> SubmitAsync(ContactSubmission submission) =>
        Task.FromResult(true);
}

public class StubSponsorshipSubmissionService : SponsorshipSubmissionService
{
    public StubSponsorshipSubmissionService() : base(null!, null!) { }

    public override Task<bool> SubmitAsync(SponsorshipSubmission submission) =>
        Task.FromResult(true);
}
