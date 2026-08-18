namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class LawyerAvailabilityResponse
{
    public Guid LawyerId { get; set; }
    public bool IsAvailable { get; set; }
}
