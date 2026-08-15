using SmartCourt.Common.Domain;
using SmartCourt.Features.Contracts.Enums;

namespace SmartCourt.Features.Contracts.Entities;

public sealed class Contract
{
    private Contract()
    {
    }

    internal Contract(
        Guid id,
        Guid proposalId,
        Guid legalCaseId,
        Guid clientUserId,
        Guid lawyerUserId,
        string title,
        string termsAndConditions,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ProposalId = EntityGuard.NotEmpty(proposalId, nameof(proposalId));
        LegalCaseId = EntityGuard.NotEmpty(legalCaseId, nameof(legalCaseId));
        ClientUserId = EntityGuard.NotEmpty(clientUserId, nameof(clientUserId));
        LawyerUserId = EntityGuard.NotEmpty(lawyerUserId, nameof(lawyerUserId));
        Title = EntityGuard.Required(title, nameof(title));
        TermsAndConditions = EntityGuard.Required(
            termsAndConditions,
            nameof(termsAndConditions));
        Currency = EntityGuard.CurrencyEgp;
        Status = ContractStatus.Draft;
        CreatedAt = createdAt;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid ProposalId { get; internal set; }
    public Guid LegalCaseId { get; internal set; }
    public Guid ClientUserId { get; internal set; }
    public Guid LawyerUserId { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string TermsAndConditions { get; internal set; } = string.Empty;
    public string Currency { get; internal set; } = EntityGuard.CurrencyEgp;
    public ContractStatus Status { get; internal set; }
    public DateTimeOffset? AcceptedByClientAt { get; internal set; }
    public DateTimeOffset? AcceptedByLawyerAt { get; internal set; }
    public DateTimeOffset? ActivatedAt { get; internal set; }
    public DateTimeOffset? CompletedAt { get; internal set; }
    public DateTimeOffset? TerminatedAt { get; internal set; }
    public string? TerminationReason { get; internal set; }
    public Guid? TerminatedByUserId { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
}
