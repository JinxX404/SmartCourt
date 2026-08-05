namespace SmartCourt.Features.Case.GetCases.DTOs
{
    public class CaseListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DocumentCount { get; set; }
    }
}
