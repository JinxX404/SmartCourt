namespace SmartCourt.Common.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public System.DateTimeOffset CreatedAt { get; set; } = System.DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public System.DateTimeOffset UpdatedAt { get; set; } = System.DateTimeOffset.UtcNow;
    public string? LastModifiedBy { get; set; }
}
