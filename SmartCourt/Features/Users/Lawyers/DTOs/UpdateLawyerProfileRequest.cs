namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class UpdateLawyerProfileRequest
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Bio { get; set; }
    public string? Address { get; set; }
}
