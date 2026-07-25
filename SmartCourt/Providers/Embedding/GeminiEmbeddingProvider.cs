using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Embedding;

public class GeminiEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiEmbeddingOptions _options;
    private readonly ILogger<GeminiEmbeddingProvider> _logger;

    public GeminiEmbeddingProvider(
        HttpClient httpClient,
        IOptions<GeminiEmbeddingOptions> options,
        ILogger<GeminiEmbeddingProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Gemini API Key is not configured. Embeddings will fail.");
        }

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public int Dimensions => _options.Dimensions;

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0) return Array.Empty<float[]>();

        var requests = new List<object>();
        foreach (var text in texts)
        {
            requests.Add(new
            {
                model = $"models/{_options.Model}",
                content = new
                {
                    parts = new[] { new { text } }
                },
                outputDimensionality = Dimensions
            });
        }

        var requestBody = new
        {
            requests
        };

        var requestUrl = $"models/{_options.Model}:batchEmbedContents?key={_options.ApiKey}";

        var response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Gemini API failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
            throw new BusinessException($"Failed to generate embeddings via Gemini: {response.ReasonPhrase}");
        }

        var responseData = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
        if (responseData == null || !responseData.RootElement.TryGetProperty("embeddings", out var embeddingsArray))
        {
            throw new BusinessException("Gemini API returned an invalid response format (missing 'embeddings').");
        }

        var results = new List<float[]>();
        foreach (var embeddingItem in embeddingsArray.EnumerateArray())
        {
            if (embeddingItem.TryGetProperty("values", out var valuesArray))
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
                throw new BusinessException("Gemini API returned an invalid embedding format (missing 'values').");
            }
        }

        if (results.Count != texts.Count)
        {
            throw new BusinessException($"Gemini API returned {results.Count} embeddings, expected {texts.Count}.");
        }

        return results;
    }
}
