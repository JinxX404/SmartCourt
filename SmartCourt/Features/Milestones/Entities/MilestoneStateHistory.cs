using SmartCourt.Common.Domain;
using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.Entities;

public sealed class MilestoneStateHistory
{
    private MilestoneStateHistory()
    {
    }

    internal MilestoneStateHistory(
        Guid id,
        Guid milestoneId,
        MilestoneStatus? previousStatus,
        MilestoneStatus newStatus,
        string trigger,
        Guid? actorUserId,
        string? reason,
        Guid correlationId,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        MilestoneId = EntityGuard.NotEmpty(milestoneId, nameof(milestoneId));
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Trigger = EntityGuard.Required(trigger, nameof(trigger));
        ActorUserId = EntityGuard.OptionalGuid(actorUserId, nameof(actorUserId));
        Reason = reason;
        CorrelationId = EntityGuard.NotEmpty(correlationId, nameof(correlationId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; private set; }
    public Guid MilestoneId { get; private set; }
    public MilestoneStatus? PreviousStatus { get; private set; }
    public MilestoneStatus NewStatus { get; private set; }
    public string Trigger { get; private set; } = string.Empty;
    public Guid? ActorUserId { get; private set; }
    public string? Reason { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
