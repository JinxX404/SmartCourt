namespace SmartCourt.Features.Case.GetCases.DTOs
{
    public class CaseListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int DocumentCount { get; set; }
        public Guid? LawyerId { get; set; }
        public Guid? LastReviewId { get; set; }
        public Guid? ChatId { get; set; }
    }
}
