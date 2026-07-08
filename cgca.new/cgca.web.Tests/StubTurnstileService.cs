using cgca.web.Services;

namespace cgca.web.Tests;

public class StubTurnstileService : TurnstileService
{
    public StubTurnstileService() : base(null!, null!, null!) { }

    public override Task<bool> VerifyAsync(string? token, string? remoteIp = null) =>
        Task.FromResult(true);
}
