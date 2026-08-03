using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.ChatModel;

public class DeepSeekChatModelProvider : IChatModelProvider
{
    private readonly HttpClient _httpClient;
    private readonly DeepSeekChatModelOptions _options;
    private readonly ILogger<DeepSeekChatModelProvider> _logger;

    public DeepSeekChatModelProvider(
        HttpClient httpClient,
        IOptions<DeepSeekChatModelOptions> options,
        ILogger<DeepSeekChatModelProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("DeepSeek API Key is not configured for Chat Model.");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
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
            model_id = _options.Model,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            },
            system_prompt = systemPrompt,
            max_tokens = _options.MaxTokens
        };

        HttpResponseMessage? response = null;
        int maxRetries = 3;
        int delayMs = 2000;

        for (int i = 0; i < maxRetries; i++)
        {
            response = await _httpClient.PostAsJsonAsync("student/chat", requestBody, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                break;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && i < maxRetries - 1)
            {
                _logger.LogWarning("DeepSeek API rate limit hit. Retrying in {Delay}ms...", delayMs);
                await Task.Delay(delayMs, cancellationToken);
                delayMs *= 2; // Exponential backoff
                continue;
            }
            
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("DeepSeek Chat API failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
            throw new BusinessException($"Failed to generate content via DeepSeek: {response.ReasonPhrase}");
        }

        if (response == null) 
        {
            throw new BusinessException("Failed to generate content via DeepSeek.");
        }

        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("DeepSeek raw response body: {RawBody}", rawBody);

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            throw new BusinessException("DeepSeek Chat API returned an empty response.");
        }

        try
        {
            using var responseData = JsonDocument.Parse(rawBody);

            // ITI Student Bedrock Gateway format: { "output_text": "..." }
            if (responseData.RootElement.TryGetProperty("output_text", out var outputTextProp))
            {
                var text = outputTextProp.GetString();
                if (!string.IsNullOrEmpty(text))
                    return text;
            }

            // Try OpenAI-like format
            if (responseData.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var message = choices[0].GetProperty("message");
                if (message.TryGetProperty("content", out var contentProp))
                {
                    return contentProp.GetString() ?? string.Empty;
                }
            }
            
            // Try Anthropic-like format
            if (responseData.RootElement.TryGetProperty("content", out var anthropicContent) && anthropicContent.ValueKind == JsonValueKind.Array && anthropicContent.GetArrayLength() > 0)
            {
                if (anthropicContent[0].TryGetProperty("text", out var textProp))
                {
                    return textProp.GetString() ?? string.Empty;
                }
            }
            
            // Try simple string or direct answer format
            if (responseData.RootElement.TryGetProperty("answer", out var answerProp))
            {
                return answerProp.GetString() ?? string.Empty;
            }
            if (responseData.RootElement.TryGetProperty("reply", out var replyProp))
            {
                return replyProp.GetString() ?? string.Empty;
            }

            // Fallback: just return the raw JSON if we can't parse it
            _logger.LogWarning("Could not extract text from DeepSeek response. Returning raw body.");
            return rawBody;
        }
        catch (JsonException ex)
        {
            // If the body isn't valid JSON at all, return it as-is (plain text response)
            _logger.LogWarning(ex, "DeepSeek response was not valid JSON. Treating as plain text.");
            return rawBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse API response: {Response}", rawBody);
            throw new BusinessException("Chat API returned an invalid response format.");
        }
    }
}
