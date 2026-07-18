namespace SmartCourt.Common.Entities;

public class LawyerProfile
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid? SpecializationId { get; set; }
    public LegalSpecialization? Specialization { get; set; }

    public int YearsOfExperience { get; set; }
    public string? Bio { get; set; }
    public string? Address { get; set; }
    public bool IsAvailable { get; set; } = true;
}
