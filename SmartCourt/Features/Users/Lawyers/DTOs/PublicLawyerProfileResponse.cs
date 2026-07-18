namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class PublicLawyerProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    
    public Guid? SpecializationId { get; set; }
    public string SpecializationName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    
    public int YearsOfExperience { get; set; }
    public SmartCourt.Common.Enums.LawyerLevel Level { get; set; }
    public string? Bio { get; set; }
    public bool IsAvailable { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string Status { get; set; } = string.Empty;
}
