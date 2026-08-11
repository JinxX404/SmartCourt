using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Interfaces.Providers;

public interface IRerankerProvider
{
    Task<IReadOnlyList<RerankedResult>> RerankAsync(
        string query,
        IReadOnlyList<string> documents,
        int topN,
        CancellationToken cancellationToken = default);
}

public record RerankedResult(int Index, float RelevanceScore);
