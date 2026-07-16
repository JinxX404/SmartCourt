namespace SmartCourt.Features.Users.Profile;

public record UserProfileResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string? ProfilePictureUrl,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    ClientProfileDto? ClientProfile,
    LawyerProfileDto? LawyerProfile
);

public record ClientProfileDto(
    DateTime? DateOfBirth,
    int NationalIdVerificationStatus,
    string NationalIdVerificationStatusName
);

public record LawyerProfileDto(
    string? Bio,
    string? OfficeAddress,
    int YearsOfExperience,
    bool IsAvailable,
    int NationalIdVerificationStatus,
    string NationalIdVerificationStatusName,
    int BarCardVerificationStatus,
    string BarCardVerificationStatusName,
    bool IsFullyVerified,
    IEnumerable<SpecializationDto> Specializations
);

public record SpecializationDto(
    string Id,
    string Name
);
