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
        Guid userId,
        Guid? caseId,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        UserId = EntityGuard.NotEmpty(userId, nameof(userId));
        CaseId = EntityGuard.OptionalGuid(caseId, nameof(caseId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
        IsDeleted = false;
    }

    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public Guid? CaseId { get; internal set; }
    public string? Title { get; internal set; }
    public string? CachedCaseContext { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
    public bool IsDeleted { get; internal set; }

    public CaseEntity? Case { get; internal set; }
    public ICollection<AgentMessage> Messages { get; internal set; } = [];

    internal static AgentConversation Create(
        Guid id,
        Guid userId,
        Guid? caseId,
        DateTime createdAt)
    {
        return new AgentConversation(id, userId, caseId, createdAt);
    }

    internal void UpdateTitle(string title, DateTime updatedAt)
    {
        Title = EntityGuard.Required(title, nameof(title)).Trim();
        UpdatedAt = EntityGuard.Utc(updatedAt, nameof(updatedAt));
    }

    internal void CacheCaseContext(string context, DateTime updatedAt)
    {
        CachedCaseContext = EntityGuard.Required(context, nameof(context));
        UpdatedAt = EntityGuard.Utc(updatedAt, nameof(updatedAt));
    }

    internal void MarkMessageAdded(DateTime sentAt)
    {
        UpdatedAt = EntityGuard.Utc(sentAt, nameof(sentAt));
    }

    internal void SoftDelete(DateTime deletedAt)
    {
        IsDeleted = true;
        UpdatedAt = EntityGuard.Utc(deletedAt, nameof(deletedAt));
    }
}
