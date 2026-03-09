using cgca.web.Models;
using cgca.web.Services;

namespace cgca.web.Tests;

public class StubChatService : IChatService
{
    public Task<ChatResponse> SendMessageAsync(ChatRequest request)
    {
        return Task.FromResult(new ChatResponse
        {
            Reply = "Test response",
            SessionId = request.SessionId
        });
    }

    public Task<bool> SubmitLeadAsync(LeadRequest lead)
    {
        return Task.FromResult(true);
    }
}
