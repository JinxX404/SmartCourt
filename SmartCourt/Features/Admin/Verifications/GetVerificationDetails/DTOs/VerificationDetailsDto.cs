namespace SmartCourt.Features.Admin.Verifications.GetVerificationDetails.DTOs;

public sealed class VerificationDetailsDto
{
    public Guid LawyerId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? NationalNumber { get; init; }
    public string? Address { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; init; }
    public string AccountStatus { get; init; } = string.Empty;
    public bool IsFullyVerified { get; init; }
    public string? Role { get; init; }

    // Lawyer Specific Profile Data
    public int? Level { get; init; }
    public IReadOnlyList<LawyerSpecializationDto> Specializations { get; init; } = [];
    public string? Bio { get; init; }

    public IReadOnlyList<VerificationDocumentDetailsDto> Documents { get; init; } = [];
    public IReadOnlyList<string> ModifiedFields { get; init; } = [];
}
