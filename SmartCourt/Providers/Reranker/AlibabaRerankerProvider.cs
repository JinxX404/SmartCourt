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

namespace SmartCourt.Providers.Reranker;

public class AlibabaRerankerProvider : IRerankerProvider
{
    private readonly HttpClient _httpClient;
    private readonly AlibabaRerankerOptions _options;
    private readonly ILogger<AlibabaRerankerProvider> _logger;

    public AlibabaRerankerProvider(
        HttpClient httpClient,
        IOptions<AlibabaRerankerOptions> options,
        ILogger<AlibabaRerankerProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Alibaba Reranker API Key is not configured.");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            var baseUrl = _options.BaseUrl.TrimEnd('/') + "/";
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                _httpClient.BaseAddress = baseUri;
            }
        }
    }

    public async Task<IReadOnlyList<RerankedResult>> RerankAsync(
        string query,
        IReadOnlyList<string> documents,
        int topN,
        CancellationToken cancellationToken = default)
    {
        if (documents == null || documents.Count == 0)
        {
            return Array.Empty<RerankedResult>();
        }

        var requestBody = new
        {
            model = _options.Model,
            query = query,
            documents = documents,
            top_n = topN
        };

        HttpResponseMessage? response = null;
        int maxRetries = 3;
        int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                response = await _httpClient.PostAsJsonAsync("reranks", requestBody, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    break;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && i < maxRetries - 1)
                {
                    _logger.LogWarning("Alibaba Reranker API rate limit hit. Retrying in {Delay}ms...", delayMs);
                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2; // Exponential backoff
                    continue;
                }

                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Alibaba Reranker API failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
                throw new BusinessException($"Failed to rerank documents via Alibaba: {response.ReasonPhrase}");
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Alibaba Reranker API request timed out on attempt {Attempt}.", i + 1);
                if (i == maxRetries - 1) throw new BusinessException("Alibaba Reranker API request timed out.", ex);
            }
            catch (Exception ex) when (ex is not BusinessException)
            {
                _logger.LogError(ex, "Failed to connect to Alibaba Reranker API on attempt {Attempt}.", i + 1);
                if (i == maxRetries - 1) throw new BusinessException($"Failed to connect to Alibaba Reranker API. Inner: {ex.Message}", ex);
            }
        }

        if (response == null || !response.IsSuccessStatusCode)
        {
            throw new BusinessException("Failed to rerank documents via Alibaba.");
        }

        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Alibaba Reranker raw response body: {RawBody}", rawBody);

        try
        {
            using var responseData = JsonDocument.Parse(rawBody);

            if (responseData.RootElement.TryGetProperty("results", out var resultsArray))
            {
                var rerankedResults = new List<RerankedResult>();
                foreach (var resultElement in resultsArray.EnumerateArray())
                {
                    if (resultElement.TryGetProperty("index", out var indexProp) &&
                        resultElement.TryGetProperty("relevance_score", out var scoreProp))
                    {
                        rerankedResults.Add(new RerankedResult(indexProp.GetInt32(), (float)scoreProp.GetDouble()));
                    }
                }
                return rerankedResults;
            }

            throw new BusinessException("Alibaba Reranker API returned an unexpected response format (missing 'results').");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Alibaba Reranker API response: {Response}", rawBody);
            throw new BusinessException("Failed to parse Alibaba Reranker API response.", ex);
        }
    }
}
