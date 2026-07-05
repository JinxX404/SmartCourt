namespace SmartCourt.Core.Common;

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public string? LastModifiedBy { get; set; }
}
