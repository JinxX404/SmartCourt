namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class UpdateLawyerProfileRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public SmartCourt.Common.Enums.LawyerLevel Level { get; set; }
    public string? Bio { get; set; }
    public string? Address { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public List<LawyerSpecializationDto>? Specializations { get; set; }
}
