using System.Collections.Generic;
using SmartCourt.Features.Case.CreateCase.DTOs;

namespace SmartCourt.Features.Case.UpdateCase.DTOs
{
    public class UpdateCaseResponse
    {
        public Guid CaseId { get; set; }
        public List<CaseDocumentUploadErrorDto> FailedDocuments { get; set; }
    }
}
