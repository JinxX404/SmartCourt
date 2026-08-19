using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Interfaces.Providers;

public interface IRerankerProvider
{
    Task<RerankResponse> RerankAsync(
        string query,
        IReadOnlyList<string> documents,
        int topN,
        CancellationToken cancellationToken = default);
}

public record RerankedResult(int Index, float RelevanceScore);

public record RerankResponse(
    IReadOnlyList<RerankedResult> Results,
    int InputTokens
);
