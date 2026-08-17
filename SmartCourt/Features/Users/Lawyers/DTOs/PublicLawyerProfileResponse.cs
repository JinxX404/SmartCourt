using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class PublicLawyerProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public SmartCourt.Common.Enums.LawyerLevel Level { get; set; }
    public string? Bio { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public bool IsAvailable { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int YearsOfExperience { get; set; }
    public string? SpecializationName { get; set; }
    public List<LawyerSpecializationDto> Specializations { get; set; } = new();
}
