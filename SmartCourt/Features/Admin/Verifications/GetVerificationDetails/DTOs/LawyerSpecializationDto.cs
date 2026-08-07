namespace SmartCourt.Features.Admin.Verifications.GetVerificationDetails.DTOs;

public sealed class LawyerSpecializationDto
{
    public int Specialization { get; init; }
    public string SpecializationName { get; init; } = string.Empty;
    public int YearsOfExperience { get; init; }
    public int CasesHandled { get; init; }
}
