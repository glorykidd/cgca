using cgca.web.client.Models;

namespace cgca.web.client.Services;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(ChatRequest request);
    Task<bool> SubmitLeadAsync(LeadRequest lead);
}
