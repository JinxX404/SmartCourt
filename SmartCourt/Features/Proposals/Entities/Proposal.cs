using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Proposals.Enums;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Features.Proposals.Entities;

public sealed class Proposal
{
    public static readonly TimeSpan ResponseWindow = TimeSpan.FromDays(3);

    private Proposal()
    {
    }

    internal Proposal(
        Guid id,
        Guid legalCaseId,
        Guid clientUserId,
        Guid lawyerUserId,
        DateTime createdAt)
        : this(id, legalCaseId, clientUserId, lawyerUserId, string.Empty, createdAt, true)
    {
    }

    internal Proposal(
        Guid id,
        Guid legalCaseId,
        Guid clientUserId,
        Guid lawyerUserId,
        string message,
        DateTime createdAt)
        : this(id, legalCaseId, clientUserId, lawyerUserId, message, createdAt, false)
    {
    }

    private Proposal(
        Guid id,
        Guid legalCaseId,
        Guid clientUserId,
        Guid lawyerUserId,
        string message,
        DateTime createdAt,
        bool allowLegacyEmptyMessage)
    {
        if (id == Guid.Empty || legalCaseId == Guid.Empty
            || clientUserId == Guid.Empty || lawyerUserId == Guid.Empty)
        {
            throw new BusinessException("Proposal identifiers are required.");
        }

        if (createdAt.Kind != DateTimeKind.Utc)
        {
            throw new BusinessException("Proposal creation time must be UTC.");
        }

        if (!allowLegacyEmptyMessage && string.IsNullOrWhiteSpace(message))
        {
            throw new BusinessException("Proposal message is required.");
        }

        if (message.Length > 2_000)
        {
            throw new BusinessException("Proposal message cannot exceed 2000 characters.");
        }

        Id = id;
        LegalCaseId = legalCaseId;
        ClientUserId = clientUserId;
        LawyerUserId = lawyerUserId;
        Message = message.Trim();
        Status = ProposalStatus.Pending;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ExpiresAt = createdAt.Add(ResponseWindow);
    }

    public Guid Id { get; internal set; }
    public Guid LegalCaseId { get; internal set; }
    public Guid ClientUserId { get; internal set; }
    public Guid LawyerUserId { get; internal set; }
    public CaseEntity Case { get; internal set; } = null!;
    public string Message { get; internal set; } = string.Empty;
    public ProposalStatus Status { get; internal set; }
    public string? DecisionReason { get; internal set; }
    public DateTime? RespondedAt { get; internal set; }
    public DateTime ExpiresAt { get; internal set; }
    public DateTime? ClosedAt { get; internal set; }
    public Guid? ClosedByUserId { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }

    internal void Accept(DateTime respondedAt)
    {
        EnsurePending();
        EnsureUtc(respondedAt);
        if (respondedAt >= ExpiresAt)
        {
            throw new BusinessException("Expired proposals cannot be accepted.");
        }

        Status = ProposalStatus.Accepted;
        DecisionReason = null;
        RespondedAt = respondedAt;
        UpdatedAt = respondedAt;
    }

    internal void Reject(string? reason, DateTime respondedAt)
    {
        EnsurePending();
        EnsureUtc(respondedAt);
        if (respondedAt >= ExpiresAt)
        {
            throw new BusinessException("Expired proposals cannot be rejected.");
        }

        Status = ProposalStatus.Rejected;
        DecisionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        RespondedAt = respondedAt;
        ClosedAt = respondedAt;
        UpdatedAt = respondedAt;
    }

    internal void Cancel(string reason, Guid clientUserId, DateTime cancelledAt)
    {
        EnsurePending();
        EnsureClosure(reason, clientUserId, cancelledAt);

        Status = ProposalStatus.Cancelled;
        DecisionReason = reason.Trim();
        ClosedByUserId = clientUserId;
        ClosedAt = cancelledAt;
        UpdatedAt = cancelledAt;
    }

    internal void Expire(DateTime expiredAt)
    {
        EnsurePending();
        EnsureUtc(expiredAt);
        if (expiredAt < ExpiresAt)
        {
            throw new BusinessException("Proposal cannot expire before its deadline.");
        }

        Status = ProposalStatus.Expired;
        ClosedAt = expiredAt;
        UpdatedAt = expiredAt;
    }

    internal void Terminate(string reason, Guid actorUserId, DateTime terminatedAt)
    {
        if (Status != ProposalStatus.Accepted)
        {
            throw new BusinessException("Only an accepted proposal can be terminated.");
        }

        EnsureClosure(reason, actorUserId, terminatedAt);
        Status = ProposalStatus.Terminated;
        DecisionReason = reason.Trim();
        ClosedByUserId = actorUserId;
        ClosedAt = terminatedAt;
        UpdatedAt = terminatedAt;
    }

    internal void Supersede(DateTime supersededAt)
    {
        if (Status is not (ProposalStatus.Pending or ProposalStatus.Accepted))
        {
            return;
        }

        EnsureUtc(supersededAt);
        Status = ProposalStatus.Superseded;
        DecisionReason = "Another contract was activated for this case.";
        ClosedAt = supersededAt;
        UpdatedAt = supersededAt;
    }

    private void EnsurePending()
    {
        if (Status != ProposalStatus.Pending)
        {
            throw new BusinessException("Only a pending proposal can be decided.");
        }
    }

    private static void EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new BusinessException("Proposal timestamps must be UTC.");
        }
    }

    private static void EnsureClosure(
        string reason,
        Guid actorUserId,
        DateTime closedAt)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new BusinessException("The user ending the proposal is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessException("A reason is required.");
        }

        if (reason.Trim().Length > 1_000)
        {
            throw new BusinessException("The reason cannot exceed 1000 characters.");
        }

        EnsureUtc(closedAt);
    }
}
