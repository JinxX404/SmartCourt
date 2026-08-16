using SmartCourt.Common.Domain;
using SmartCourt.Features.Contracts.Enums;

namespace SmartCourt.Features.Contracts.Entities;

public sealed class ContractStateHistory
{
    private ContractStateHistory()
    {
    }

    internal ContractStateHistory(
        Guid id,
        Guid contractId,
        ContractStatus? previousStatus,
        ContractStatus newStatus,
        string trigger,
        Guid? actorUserId,
        string? reason,
        Guid correlationId,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Trigger = EntityGuard.Required(trigger, nameof(trigger));
        ActorUserId = EntityGuard.OptionalGuid(actorUserId, nameof(actorUserId));
        Reason = reason;
        CorrelationId = EntityGuard.NotEmpty(correlationId, nameof(correlationId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; private set; }
    public Guid ContractId { get; private set; }
    public ContractStatus? PreviousStatus { get; private set; }
    public ContractStatus NewStatus { get; private set; }
    public string Trigger { get; private set; } = string.Empty;
    public Guid? ActorUserId { get; private set; }
    public string? Reason { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
