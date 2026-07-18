using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using SmartCourt.Entities;
using SmartCourt.Features.Auth.Enums;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string NationalNumber { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Unverified;
    public LawyerProfile? LawyerProfile { get; set; }
    public ClientProfile? ClientProfile { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<UserVerificationDocument> VerificationDocuments { get; set; }
        = new List<UserVerificationDocument>();
    public string? ProfilePictureUrl { get; set; }
}
