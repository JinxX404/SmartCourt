using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Features.Proposals.Enums;

namespace SmartCourt.Features.Proposals.Entities;

public sealed class Proposal
{
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
    }

    public Guid Id { get; internal set; }
    public Guid LegalCaseId { get; internal set; }
    public Guid ClientUserId { get; internal set; }
    public Guid LawyerUserId { get; internal set; }
    public LegalCase LegalCase { get; internal set; } = null!;
    public string Message { get; internal set; } = string.Empty;
    public ProposalStatus Status { get; internal set; }
    public string? DecisionReason { get; internal set; }
    public DateTime? RespondedAt { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }

    internal void Accept(DateTime respondedAt)
    {
        EnsurePending();
        EnsureUtc(respondedAt);

        Status = ProposalStatus.Accepted;
        DecisionReason = null;
        RespondedAt = respondedAt;
        UpdatedAt = respondedAt;
    }

    internal void Reject(string? reason, DateTime respondedAt)
    {
        EnsurePending();
        EnsureUtc(respondedAt);

        Status = ProposalStatus.Rejected;
        DecisionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        RespondedAt = respondedAt;
        UpdatedAt = respondedAt;
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
}
