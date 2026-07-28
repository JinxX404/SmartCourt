using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.ChatModel;

public class GeminiChatModelProvider : IChatModelProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiChatModelOptions _options;
    private readonly ILogger<GeminiChatModelProvider> _logger;

    public GeminiChatModelProvider(
        HttpClient httpClient,
        IOptions<GeminiChatModelOptions> options,
        ILogger<GeminiChatModelProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Gemini API Key is not configured for Chat Model.");
        }

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
    }

    public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = userPrompt } }
                }
            }
        };

        var requestUrl = $"models/{_options.Model}:generateContent?key={_options.ApiKey}";

        HttpResponseMessage? response = null;
        int maxRetries = 3;
        int delayMs = 2000;

        for (int i = 0; i < maxRetries; i++)
        {
            response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                break;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && i < maxRetries - 1)
            {
                _logger.LogWarning("Gemini API rate limit hit. Retrying in {Delay}ms...", delayMs);
                await Task.Delay(delayMs, cancellationToken);
                delayMs *= 2; // Exponential backoff
                continue;
            }
            
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Gemini Chat API failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
            throw new BusinessException($"Failed to generate content via Gemini: {response.ReasonPhrase}");
        }

        if (response == null) 
        {
            throw new BusinessException("Failed to generate content via Gemini.");
        }

        var responseData = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
        if (responseData == null)
        {
            throw new BusinessException("Gemini Chat API returned an empty response.");
        }

        try
        {
            var text = responseData.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response: {Response}", responseData.RootElement.ToString());
            throw new BusinessException("Gemini Chat API returned an invalid response format.");
        }
    }
}
