using System;
using System.Collections.Generic;
using SmartCourt.Features.Case.CreateCase.DTOs;

namespace SmartCourt.Features.Case.AddCaseDocument.DTOs;

public class AddCaseDocumentResponse
{
    public Guid CaseId { get; set; }
    public List<AddedDocumentDto> AddedDocuments { get; set; } = new();
    public List<CaseDocumentUploadErrorDto> FailedDocuments { get; set; } = new();
}
