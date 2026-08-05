using SmartCourt.Common.Domain;
using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.Entities;

public sealed class LawyerPenalty
{
    private LawyerPenalty()
    {
    }

    internal LawyerPenalty(
        Guid id,
        Guid lawyerUserId,
        Guid disputeId,
        PenaltyType penaltyType,
        string reason,
        DateTime startsAt,
        DateTime? endsAt,
        Guid createdByUserId,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        LawyerUserId = EntityGuard.NotEmpty(
            lawyerUserId,
            nameof(lawyerUserId));
        DisputeId = EntityGuard.NotEmpty(disputeId, nameof(disputeId));
        PenaltyType = penaltyType;
        Reason = EntityGuard.Required(reason, nameof(reason));
        StartsAt = EntityGuard.Utc(startsAt, nameof(startsAt));
        EndsAt = EntityGuard.OptionalUtc(endsAt, nameof(endsAt));
        CreatedByUserId = EntityGuard.NotEmpty(
            createdByUserId,
            nameof(createdByUserId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; private set; }
    public Guid LawyerUserId { get; private set; }
    public Guid DisputeId { get; private set; }
    public PenaltyType PenaltyType { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
