using SmartCourt.Common.Domain;
using SmartCourt.Infrastructure.Persistence.Enums;

namespace SmartCourt.Infrastructure.Persistence.Entities;

public sealed class IdempotencyRecord
{
    private IdempotencyRecord()
    {
    }

    internal IdempotencyRecord(
        Guid id,
        Guid userId,
        string key,
        string operation,
        string resourceType,
        Guid resourceId,
        string requestHash,
        DateTime expiresAt,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        UserId = EntityGuard.NotEmpty(userId, nameof(userId));
        Key = EntityGuard.Required(key, nameof(key));
        Operation = EntityGuard.Required(operation, nameof(operation));
        ResourceType = EntityGuard.Required(resourceType, nameof(resourceType));
        ResourceId = EntityGuard.NotEmpty(resourceId, nameof(resourceId));
        RequestHash = EntityGuard.Required(requestHash, nameof(requestHash));
        Status = IdempotencyStatus.Processing;
        ExpiresAt = EntityGuard.Utc(expiresAt, nameof(expiresAt));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public string Key { get; internal set; } = string.Empty;
    public string Operation { get; internal set; } = string.Empty;
    public string ResourceType { get; internal set; } = string.Empty;
    public Guid ResourceId { get; internal set; }
    public string RequestHash { get; internal set; } = string.Empty;
    public IdempotencyStatus Status { get; internal set; }
    public int? ResponseStatusCode { get; internal set; }
    public string? ResponseBody { get; internal set; }
    public Guid? ResultReferenceId { get; internal set; }
    public DateTime ExpiresAt { get; internal set; }
    public DateTime? CompletedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTime CreatedAt { get; internal set; }
}
