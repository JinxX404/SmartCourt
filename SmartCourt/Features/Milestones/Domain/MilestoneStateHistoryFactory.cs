using SmartCourt.Common.Domain;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.Domain;

internal static class MilestoneStateHistoryFactory
{
    internal static MilestoneStateHistory Create(
        Guid id,
        Guid milestoneId,
        MilestoneStatus previousStatus,
        MilestoneStatus newStatus,
        string trigger,
        Guid? actorUserId,
        string reason,
        Guid correlationId,
        DateTimeOffset occurredAt)
    {
        MilestoneTransitionGuard.EnsureCanTransition(
            previousStatus,
            newStatus);

        return new MilestoneStateHistory(
            EntityGuard.NotEmpty(id, nameof(id)),
            EntityGuard.NotEmpty(milestoneId, nameof(milestoneId)),
            previousStatus,
            newStatus,
            EntityGuard.Required(trigger, nameof(trigger)),
            EntityGuard.OptionalGuid(actorUserId, nameof(actorUserId)),
            EntityGuard.Required(reason, nameof(reason)),
            EntityGuard.NotEmpty(correlationId, nameof(correlationId)),
            EntityGuard.Utc(occurredAt, nameof(occurredAt)));
    }
}
