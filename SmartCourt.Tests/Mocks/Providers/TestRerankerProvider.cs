using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Tests.Mocks.Providers;

public class TestRerankerProvider : IRerankerProvider
{
    public IReadOnlyList<RerankedResult>? ResultsToReturn { get; set; }

    public Task<RerankResponse> RerankAsync(string query, IReadOnlyList<string> documents, int topN, CancellationToken cancellationToken = default)
    {
        // Simple dummy implementation: just return them in original order, with a mock score.
        var results = documents.Take(topN).Select((doc, i) => new RerankedResult(i, 0.9f - (i * 0.1f))).ToList();
        return Task.FromResult(new RerankResponse(results, query.Length + documents.Sum(d => d.Length)));
    }
}
