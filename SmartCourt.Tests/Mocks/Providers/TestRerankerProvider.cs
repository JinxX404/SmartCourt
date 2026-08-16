using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Tests.Mocks.Providers;

public class TestRerankerProvider : IRerankerProvider
{
    public IReadOnlyList<RerankedResult>? ResultsToReturn { get; set; }

    public Task<IReadOnlyList<RerankedResult>> RerankAsync(string query, IReadOnlyList<string> documents, int topN, CancellationToken cancellationToken = default)
    {
        if (ResultsToReturn != null)
        {
            return Task.FromResult(ResultsToReturn);
        }

        var results = new List<RerankedResult>();
        for (int i = 0; i < Math.Min(documents.Count, topN); i++)
        {
            results.Add(new RerankedResult(i, 0.9f));
        }

        return Task.FromResult<IReadOnlyList<RerankedResult>>(results);
    }
}
