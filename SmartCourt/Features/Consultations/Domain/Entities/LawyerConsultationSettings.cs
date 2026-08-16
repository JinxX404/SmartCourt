namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class LawyerConsultationSettings
{
    public Guid LawyerId { get; set; }
    public bool IsEnabled { get; set; }
    public int MinimumBookingNoticeHours { get; set; } = 2;
    public int MaximumAdvanceBookingDays { get; set; } = 60;
    public int BufferMinutes { get; set; } = 15;
    public string TimeZoneId { get; set; } = "Africa/Cairo";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
