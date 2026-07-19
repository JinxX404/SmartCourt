namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class UpdateLawyerProfileRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Guid SpecializationId { get; set; }
    public int YearsOfExperience { get; set; }
    public SmartCourt.Common.Enums.LawyerLevel Level { get; set; }
    public string? Bio { get; set; }
    public string? Address { get; set; }
}
