using SmartCourt.Entities;

namespace SmartCourt.Common.Entities;

public class LegalSpecialization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public Guid CategoryId { get; set; }
    public LegalCategory Category { get; set; } = null!;
}
