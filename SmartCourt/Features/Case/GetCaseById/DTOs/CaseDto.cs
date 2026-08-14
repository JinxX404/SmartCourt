using System.Collections.Generic;

namespace SmartCourt.Features.Case.GetCaseById.DTOs
{
    public class CaseDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? LawyerId { get; set; }
        public Guid? LastReviewId { get; set; }
        public Guid? ChatId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Governorate { get; set; }
        public string? City { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CaseDocumentDto> Documents { get; set; }
    }

    public class CaseDocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string ContentType { get; set; }
    }
}
