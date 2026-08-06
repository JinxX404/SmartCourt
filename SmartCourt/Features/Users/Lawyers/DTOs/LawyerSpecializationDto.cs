using SmartCourt.Common.Enums;

namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class LawyerSpecializationDto
{
    public Specialization Specialization { get; set; }
    public int YearsOfExperience { get; set; }
    public int CasesHandled { get; set; }
}
