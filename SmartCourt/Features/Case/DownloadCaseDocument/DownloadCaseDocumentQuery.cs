using MediatR;

namespace SmartCourt.Features.Case.DownloadCaseDocument;

public class DownloadCaseDocumentQuery : IRequest<DownloadCaseDocumentResult>
{
    public Guid CaseId { get; set; }
    public Guid DocumentId { get; set; }
}
