using cgca.web.Models;

namespace cgca.web.Services;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(ChatRequest request);
    Task<bool> SubmitLeadAsync(LeadRequest lead);
}
