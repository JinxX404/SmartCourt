namespace SmartCourt.Features.Case.CreateCase.DTOs
{
    public class CreateCaseResponse
    {
        public Guid CaseId { get; set; }
        public List<CaseDocumentUploadErrorDto> FailedDocuments { get; set; }
    }
}
