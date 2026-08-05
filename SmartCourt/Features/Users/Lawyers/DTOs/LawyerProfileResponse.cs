using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class LawyerProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string NationalNumber { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public SmartCourt.Common.Enums.LawyerLevel Level { get; set; }
    public string? Bio { get; set; }
    public string? Address { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public List<LawyerSpecializationDto> Specializations { get; set; } = new();
}
