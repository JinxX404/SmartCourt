using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.DocumentReview.DTOs;

namespace SmartCourt.Features.DocumentReview;

public interface IDocumentReviewService
{
    Task<AnalyzeResponse> AnalyzeTextAsync(AnalyzeTextRequest request, CancellationToken cancellationToken = default);
    Task<AnalyzeResponse> AnalyzeDocumentAsync(AnalyzeDocumentRequest request, CancellationToken cancellationToken = default);
    Task<AnalyzeResponse> AskLawAsync(AskLawRequest request, CancellationToken cancellationToken = default);
}
