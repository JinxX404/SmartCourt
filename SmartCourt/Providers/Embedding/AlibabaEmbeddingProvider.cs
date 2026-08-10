using System;
using System.Collections.Generic;
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

namespace SmartCourt.Providers.Embedding;

public class AlibabaEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly AlibabaEmbeddingOptions _options;
    private readonly ILogger<AlibabaEmbeddingProvider> _logger;

    public AlibabaEmbeddingProvider(
        HttpClient httpClient,
        IOptions<AlibabaEmbeddingOptions> options,
        ILogger<AlibabaEmbeddingProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Alibaba API Key is not configured. Embeddings will fail.");
        }

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            var baseUrl = _options.BaseUrl;
            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public int Dimensions => _options.Dimensions;

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0) return Array.Empty<float[]>();

        var requestBody = new
        {
            model = _options.Model,
            input = texts,
            dimensions = Dimensions
        };

        string requestUrl = "embeddings"; 
        
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
                _logger.LogWarning("Alibaba Embedding API rate limit hit. Retrying in {Delay}ms...", delayMs);
                await Task.Delay(delayMs, cancellationToken);
                delayMs *= 2; // Exponential backoff
                continue;
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Alibaba API failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
            throw new BusinessException($"Failed to generate embeddings via Alibaba: {response.ReasonPhrase}");
        }

        if (response == null)
        {
            throw new BusinessException("Failed to generate embeddings via Alibaba.");
        }

        var responseData = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
        if (responseData == null || !responseData.RootElement.TryGetProperty("data", out var dataArray))
        {
            throw new BusinessException("Alibaba API returned an invalid response format (missing 'data').");
        }

        var results = new List<float[]>();
        foreach (var embeddingItem in dataArray.EnumerateArray())
        {
            if (embeddingItem.TryGetProperty("embedding", out var valuesArray))
            {
                var vector = new float[valuesArray.GetArrayLength()];
                int i = 0;
                foreach (var val in valuesArray.EnumerateArray())
                {
                    vector[i++] = val.GetSingle();
                }
                results.Add(vector);
            }
            else
            {
                throw new BusinessException("Alibaba API returned an invalid embedding format (missing 'embedding').");
            }
        }

        if (results.Count != texts.Count)
        {
            throw new BusinessException($"Alibaba API returned {results.Count} embeddings, expected {texts.Count}.");
        }

        return results;
    }
}
