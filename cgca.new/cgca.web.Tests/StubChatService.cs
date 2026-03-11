using cgca.web.client.Models;
using cgca.web.client.Services;

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
