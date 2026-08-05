using SmartCourt.Common.Domain;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;

namespace SmartCourt.Features.Contracts.Domain;

internal static class ContractStateHistoryFactory
{
    internal static ContractStateHistory Create(
        Guid id,
        Guid contractId,
        ContractStatus previousStatus,
        ContractStatus newStatus,
        string trigger,
        Guid? actorUserId,
        string reason,
        Guid correlationId,
        DateTime occurredAt)
    {
        ContractTransitionGuard.EnsureCanTransition(
            previousStatus,
            newStatus);

        return new ContractStateHistory(
            EntityGuard.NotEmpty(id, nameof(id)),
            EntityGuard.NotEmpty(contractId, nameof(contractId)),
            previousStatus,
            newStatus,
            EntityGuard.Required(trigger, nameof(trigger)),
            EntityGuard.OptionalGuid(actorUserId, nameof(actorUserId)),
            EntityGuard.Required(reason, nameof(reason)),
            EntityGuard.NotEmpty(correlationId, nameof(correlationId)),
            EntityGuard.Utc(occurredAt, nameof(occurredAt)));
    }
}
