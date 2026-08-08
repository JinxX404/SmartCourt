namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class UpdateLawyerProfileRequest
{
    public DateOnly? DateOfBirth { get; set; }
    public SmartCourt.Features.Auth.Enums.Gender? Gender { get; set; }
    public SmartCourt.Common.Enums.LawyerLevel Level { get; set; }
    public int YearsOfExperience { get; set; }
    public Guid? SpecializationId { get; set; }
    public string? Bio { get; set; }
    public string? Address { get; set; }
    public string? NationalNumber { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public List<LawyerSpecializationDto>? Specializations { get; set; }
}
