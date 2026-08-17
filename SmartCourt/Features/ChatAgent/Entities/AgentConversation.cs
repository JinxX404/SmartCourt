using SmartCourt.Common.Domain;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Features.ChatAgent.Entities;

public sealed class AgentConversation
{
    private AgentConversation()
    {
    }

    internal AgentConversation(
        Guid id,
        Guid? userId,
        Guid? caseId,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        UserId = EntityGuard.OptionalGuid(userId, nameof(userId));
        CaseId = EntityGuard.OptionalGuid(caseId, nameof(caseId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
        IsDeleted = false;
    }

    public Guid Id { get; internal set; }
    public Guid? UserId { get; internal set; }
    public Guid? CaseId { get; internal set; }
    public string? Title { get; internal set; }
    public string? CachedCaseContext { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
    public bool IsDeleted { get; internal set; }

    public CaseEntity? Case { get; internal set; }
    public ICollection<AgentMessage> Messages { get; internal set; } = [];

    internal static AgentConversation Create(
        Guid id,
        Guid? userId,
        Guid? caseId,
        DateTimeOffset createdAt)
    {
        return new AgentConversation(id, userId, caseId, createdAt);
    }

    internal void UpdateTitle(string title, DateTimeOffset updatedAt)
    {
        Title = EntityGuard.Required(title, nameof(title)).Trim();
        UpdatedAt = EntityGuard.Utc(updatedAt, nameof(updatedAt));
    }

    internal void CacheCaseContext(string context, DateTimeOffset updatedAt)
    {
        CachedCaseContext = EntityGuard.Required(context, nameof(context));
        UpdatedAt = EntityGuard.Utc(updatedAt, nameof(updatedAt));
    }

    internal void MarkMessageAdded(DateTimeOffset sentAt)
    {
        UpdatedAt = EntityGuard.Utc(sentAt, nameof(sentAt));
    }

    internal void SoftDelete(DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        UpdatedAt = EntityGuard.Utc(deletedAt, nameof(deletedAt));
    }
}
