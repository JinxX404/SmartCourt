namespace SmartCourt.Features.Admin.Verifications.GetVerificationDetails.DTOs;

public sealed class VerificationDetailsDto
{
    public Guid LawyerId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string AccountStatus { get; init; } = string.Empty;
    public bool IsFullyVerified { get; init; }
    public IReadOnlyList<VerificationDocumentDetailsDto> Documents { get; init; } = [];
}
