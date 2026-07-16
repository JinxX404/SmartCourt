using Microsoft.AspNetCore.Identity;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.RefreshToken;


public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string NationalNumber { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Unverified;
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
