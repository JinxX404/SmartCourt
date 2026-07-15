using Microsoft.AspNetCore.Identity;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Auth;

public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// User's full name (required, max 150 characters)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Egyptian National ID number (required, 14 digits)
    /// </summary>
    public string NationalNumber { get; set; } = string.Empty;

    /// <summary>
    /// User's gender (optional, max 20 characters)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// User's date of birth (optional)
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// User's residential or office address (optional, max 500 characters)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// User's status (required, defaults to PendingReview)
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.PendingReview;
}
