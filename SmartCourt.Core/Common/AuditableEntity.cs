namespace SmartCourt.Core.Common;

public abstract class AuditableEntity : BaseEntity
{
    public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public System.DateTime UpdatedAt { get; set; } = System.DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
}
