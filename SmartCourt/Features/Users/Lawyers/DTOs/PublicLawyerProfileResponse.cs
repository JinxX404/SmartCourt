using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class PublicLawyerProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public SmartCourt.Common.Enums.LawyerLevel Level { get; set; }
    public string? Bio { get; set; }
    public bool IsAvailable { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
