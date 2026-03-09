using System.Net.Http.Json;
using cgca.web.Models;
using Microsoft.Extensions.Configuration;

namespace cgca.web.Services;

public class ChatService : IChatService
{
    private readonly HttpClient _httpClient;

    public ChatService(IConfiguration configuration)
    {
        var baseUrl = configuration["ChatApiBaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("ChatApiBaseUrl is not configured in appsettings.json");

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChatResponse>()
            ?? new ChatResponse { Reply = "Sorry, something went wrong." };
    }

    public async Task<bool> SubmitLeadAsync(LeadRequest lead)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/leads", lead);
        return response.IsSuccessStatusCode;
    }
}
