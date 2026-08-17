using System;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.AddCaseDocument.DTOs;

namespace SmartCourt.Features.Case.AddCaseDocument;

public interface IAddCaseDocumentService
{
    Task<ApiResponse<AddCaseDocumentResponse>> AddDocumentsAsync(Guid caseId, AddCaseDocumentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AddedDocumentDto>> AddStoredDocumentAsync(Guid caseId, AddStoredCaseDocumentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteDocumentAsync(Guid caseId, Guid documentId, CancellationToken cancellationToken = default);
}
