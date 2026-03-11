namespace cgca.web.client.Models;

public class ChatRequest
{
    public List<ChatMessage> Messages { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;
}
