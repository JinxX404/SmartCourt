using SmartCourt.Entities;

namespace SmartCourt.Common.Entities;

public class LegalCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public ICollection<LegalSpecialization> Specializations { get; set; } = new List<LegalSpecialization>();
}
