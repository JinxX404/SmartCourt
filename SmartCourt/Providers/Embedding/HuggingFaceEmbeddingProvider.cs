using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Embedding;

public class HuggingFaceEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly HuggingFaceEmbeddingOptions _options;
    private readonly ILogger<HuggingFaceEmbeddingProvider> _logger;

    public HuggingFaceEmbeddingProvider(
        HttpClient httpClient,
        IOptions<HuggingFaceEmbeddingOptions> options,
        ILogger<HuggingFaceEmbeddingProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("HuggingFace API Key is not configured. Embeddings may fail if the model is not public or rate limits are hit.");
        }

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public int Dimensions => _options.Dimensions;

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0) return Array.Empty<float[]>();

        var requestBody = new
        {
            inputs = texts,
            options = new { wait_for_model = true }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        // Use the model as the path
        var response = await _httpClient.PostAsync(_options.Model, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("HuggingFace API failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
            throw new BusinessException($"Failed to generate embeddings: {response.ReasonPhrase}");
        }

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        // HF typically returns a list of float arrays (one for each input string)
        var embeddings = await JsonSerializer.DeserializeAsync<List<float[]>>(responseStream, cancellationToken: cancellationToken);

        if (embeddings == null || embeddings.Count != texts.Count)
        {
            throw new BusinessException("HuggingFace API returned invalid embedding format.");
        }

        return embeddings;
    }
}
