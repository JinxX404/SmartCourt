using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
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
        DateTimeOffset startsAt,
        DateTimeOffset? endsAt,
        Guid createdByUserId,
        DateTimeOffset createdAt)
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
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset? EndsAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? RevokedByUserId { get; private set; }
    public string? RevocationReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    internal void Revoke(Guid revokedByUserId, string reason, DateTimeOffset now)
    {
        if (IsRevoked)
        {
            throw new BusinessException("تم إلغاء هذه العقوبة مسبقًا.");
        }

        IsRevoked = true;
        RevokedByUserId = EntityGuard.NotEmpty(revokedByUserId, nameof(revokedByUserId));
        RevocationReason = EntityGuard.Required(reason, nameof(reason));
        RevokedAt = EntityGuard.Utc(now, nameof(now));
    }
}
