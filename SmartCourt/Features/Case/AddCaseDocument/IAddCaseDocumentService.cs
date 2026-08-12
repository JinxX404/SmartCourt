using System;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.AddCaseDocument.DTOs;

namespace SmartCourt.Features.Case.AddCaseDocument;

public interface IAddCaseDocumentService
{
    Task<ApiResponse<AddCaseDocumentResponse>> AddDocumentsAsync(Guid caseId, AddCaseDocumentRequest request, CancellationToken cancellationToken = default);
}
