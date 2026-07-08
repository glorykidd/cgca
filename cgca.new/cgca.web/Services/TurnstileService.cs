using System.Text.Json.Serialization;

namespace cgca.web.Services;

public class TurnstileService
{
    private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<TurnstileService> _logger;

    public TurnstileService(HttpClient http, IConfiguration config, ILogger<TurnstileService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public virtual async Task<bool> VerifyAsync(string? token, string? remoteIp = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var secretKey = _config["Turnstile:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            _logger.LogError("Turnstile:SecretKey is not configured; failing verification closed.");
            return false;
        }

        try
        {
            var fields = new Dictionary<string, string>
            {
                ["secret"] = secretKey,
                ["response"] = token
            };
            if (!string.IsNullOrWhiteSpace(remoteIp))
                fields["remoteip"] = remoteIp;

            using var response = await _http.PostAsync(VerifyUrl, new FormUrlEncodedContent(fields));
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TurnstileVerifyResponse>();
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Turnstile verification request failed; failing closed.");
            return false;
        }
    }

    private class TurnstileVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
