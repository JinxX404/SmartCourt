using SmartCourt.Common.Enums;

namespace SmartCourt.Common.Entities;

public class LawyerSpecialization
{
    public Guid Id { get; set; }
    public Guid LawyerProfileUserId { get; set; }
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public Specialization Specialization { get; set; }
    public int YearsOfExperience { get; set; }
    public int CasesHandled { get; set; }
}
