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
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        }

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            var baseUrlStr = _options.BaseUrl;
            if (baseUrlStr.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                baseUrlStr = "http://" + baseUrlStr.Substring(8);
            }

            if (Uri.TryCreate(baseUrlStr, UriKind.Absolute, out var baseUri))
            {
                var uriBuilder = new UriBuilder(baseUri);
                uriBuilder.Scheme = Uri.UriSchemeHttp;
                if (uriBuilder.Port == 443)
                {
                    uriBuilder.Port = -1;
                }
                _httpClient.BaseAddress = uriBuilder.Uri;
            }
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
            max_tokens = 300
        };

        HttpResponseMessage? response = null;
        int maxRetries = 3;
        int delayMs = 2000;

        for (int i = 0; i < maxRetries; i++)
        {
            string url = "student/chat";
            if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                var baseUrlStr = _options.BaseUrl;
                if (baseUrlStr.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    baseUrlStr = "http://" + baseUrlStr.Substring(8);
                }
                url = $"{baseUrlStr.TrimEnd('/')}/{url}";
            }

            response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

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

        var responseData = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
        if (responseData == null)
        {
            throw new BusinessException("DeepSeek Chat API returned an empty response.");
        }

        try
        {
            // Try OpenAI-like format first
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
            return responseData.RootElement.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse API response: {Response}", responseData.RootElement.ToString());
            throw new BusinessException("Chat API returned an invalid response format.");
        }
    }
}
