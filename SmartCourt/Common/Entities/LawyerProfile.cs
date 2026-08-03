using SmartCourt.Common.Enums;

namespace SmartCourt.Common.Entities;

public class LawyerProfile
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public ICollection<LawyerSpecialization> Specializations { get; set; } = new List<LawyerSpecialization>();
    public LawyerLevel Level { get; set; }
    public string? Bio { get; set; }
    public bool IsAvailable { get; set; } = true;
    public decimal AverageRating { get; set; }
    public decimal AverageResponseTimeHours { get; set; }
}

